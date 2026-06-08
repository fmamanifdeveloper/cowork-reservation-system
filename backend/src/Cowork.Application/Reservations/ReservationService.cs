using Cowork.Application.Cancellations;
using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Application.Pricing;
using Cowork.Domain.Entities;
using Cowork.Domain.Enums;
using Cowork.Domain.Rules;
using System.Text.Json;

namespace Cowork.Application.Reservations;

public sealed class ReservationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ISpaceRepository _spaceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationCodeGenerator _reservationCodeGenerator;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DynamicPricingCalculator _pricingCalculator;
    private readonly CancellationPolicyService _cancellationPolicyService;

    public ReservationService(
        ISpaceRepository spaceRepository,
        ICustomerRepository customerRepository,
        IReservationRepository reservationRepository,
        IReservationCodeGenerator reservationCodeGenerator,
        IAuditLogger auditLogger,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        DynamicPricingCalculator pricingCalculator,
        CancellationPolicyService cancellationPolicyService)
    {
        _spaceRepository = spaceRepository;
        _customerRepository = customerRepository;
        _reservationRepository = reservationRepository;
        _reservationCodeGenerator = reservationCodeGenerator;
        _auditLogger = auditLogger;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _pricingCalculator = pricingCalculator;
        _cancellationPolicyService = cancellationPolicyService;
    }

    public async Task<IReadOnlyList<ReservationDto>> ListAsync(CancellationToken cancellationToken)
    {
        var currentRole = _currentUserService.Role;
        var currentCustomerId = _currentUserService.CustomerId;

        IReadOnlyList<Reservation> reservations;

        if (currentRole == AppUserRole.Customer)
        {
            if (currentCustomerId is null)
                throw new BusinessRuleException("Customer account is not linked to a customer profile.");

            reservations = await _reservationRepository.ListByCustomerIdAsync(
                currentCustomerId.Value,
                cancellationToken);
        }
        else
        {
            reservations = await _reservationRepository.ListAsync(cancellationToken);
        }

        return reservations.Select(ToDto).ToList();
    }

    public async Task<ReservationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation was not found.");

        EnsureReservationCanBeAccessed(reservation);

        return ToDto(reservation);
    }

    public async Task<ReservationDto> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var currentCustomerId = _currentUserService.CustomerId;
        var currentRole = _currentUserService.Role;

        var customerId = request.CustomerId;

        if (currentRole == AppUserRole.Customer)
        {
            if (currentCustomerId is null)
                throw new BusinessRuleException("Customer account is not linked to a customer profile.");

            customerId = currentCustomerId.Value;
        }

        if (request.SpaceId == Guid.Empty)
            throw new BusinessRuleException("Space id is required.");

        if (customerId == Guid.Empty)
            throw new BusinessRuleException("Customer id is required.");

        var space = await _spaceRepository.GetByIdAsync(
            request.SpaceId,
            cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        if (!space.IsAvailableForReservation())
            throw new BusinessRuleException("The selected space is not available for reservations.");

        var customer = await _customerRepository.GetByIdAsync(
            customerId,
            cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer was not found.");

        ValidateReservationSchedule(
            space,
            request.StartTime,
            request.EndTime);

        var startTimeUtc = request.StartTime.ToUniversalTime();
        var endTimeUtc = request.EndTime.ToUniversalTime();

        var hasOverlap = await _reservationRepository.ExistsOverlappingAsync(
            space.Id,
            startTimeUtc,
            endTimeUtc,
            cancellationToken);

        if (hasOverlap)
        {
            throw new ReservationConflictException(
                "The selected space is already reserved for the requested time range.");
        }

        var createdAt = DateTimeOffset.UtcNow;

        var pricingResult = _pricingCalculator.Calculate(
            space,
            request.StartTime,
            request.EndTime,
            createdAt);

        var pricingBreakdown = BuildPricingBreakdown(pricingResult);

        var reservation = new Reservation(
            Guid.NewGuid(),
            _reservationCodeGenerator.Generate(createdAt),
            space.Id,
            customer.Id,
            currentUserId,
            startTimeUtc,
            endTimeUtc,
            pricingResult.BaseAmount,
            pricingResult.FinalAmount,
            pricingBreakdown,
            createdAt);

        _reservationRepository.Add(reservation);

        await _auditLogger.LogAsync(
            "ReservationCreated",
            "Reservation",
            reservation.Id,
            currentUserId,
            customer.Id,
            "Create",
            "Reservation was created.",
            null,
            new
            {
                reservation.Id,
                reservation.ReservationCode,
                reservation.SpaceId,
                reservation.CustomerId,
                reservation.StartTime,
                reservation.EndTime,
                reservation.Status,
                reservation.BaseAmount,
                reservation.FinalAmount
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(reservation);
    }

    public async Task<ReservationDto> CancelAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var reservation = await _reservationRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation was not found.");

        EnsureReservationCanBeAccessed(reservation);

        var oldValues = new
        {
            reservation.Status,
            reservation.RefundAmount,
            reservation.CancelledAt
        };

        var cancellationRequestedAt = DateTimeOffset.UtcNow;

        var refund = _cancellationPolicyService.CalculateRefund(
            reservation.StartTime,
            cancellationRequestedAt,
            reservation.FinalAmount);

        reservation.Cancel(
            refund.RefundAmount,
            cancellationRequestedAt,
            currentUserId);

        await _auditLogger.LogAsync(
            "ReservationCancelled",
            "Reservation",
            reservation.Id,
            currentUserId,
            reservation.CustomerId,
            "Cancel",
            "Reservation was cancelled.",
            oldValues,
            new
            {
                reservation.Status,
                reservation.RefundAmount,
                reservation.CancelledAt,
                reservation.CancelledByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(reservation);
    }

    public async Task<ReservationDto> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var reservation = await _reservationRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation was not found.");

        var currentRole = _currentUserService.Role;

        if (currentRole == AppUserRole.Customer)
            throw new BusinessRuleException("Customers cannot complete reservations.");

        var oldValues = new
        {
            reservation.Status,
            reservation.CompletedAt
        };

        reservation.Complete(
            DateTimeOffset.UtcNow,
            currentUserId);

        await _auditLogger.LogAsync(
            "ReservationCompleted",
            "Reservation",
            reservation.Id,
            currentUserId,
            reservation.CustomerId,
            "Complete",
            "Reservation was completed.",
            oldValues,
            new
            {
                reservation.Status,
                reservation.CompletedAt,
                reservation.CompletedByUserId
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(reservation);
    }

    private void EnsureReservationCanBeAccessed(Reservation reservation)
    {
        var currentRole = _currentUserService.Role;

        if (currentRole != AppUserRole.Customer)
            return;

        var currentCustomerId = _currentUserService.CustomerId;

        if (currentCustomerId is null || reservation.CustomerId != currentCustomerId.Value)
            throw new BusinessRuleException("Reservation does not belong to the current customer.");
    }

    private static string BuildPricingBreakdown(PricingCalculationResult pricingResult)
    {
        var breakdown = new
        {
            pricingResult.BaseAmount,
            pricingResult.FinalAmount,
            pricingResult.Adjustments
        };

        return JsonSerializer.Serialize(breakdown, JsonOptions);
    }

    private static void ValidateReservationSchedule(
        Space space,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        if (startTime >= endTime)
            throw new BusinessRuleException("Reservation start time must be earlier than end time.");

        if (!ScheduleRules.IsThirtyMinuteStep(startTime) ||
            !ScheduleRules.IsThirtyMinuteStep(endTime))
        {
            throw new BusinessRuleException("Reservation times must use 30-minute intervals.");
        }

        var duration = endTime - startTime;

        if (duration < TimeSpan.FromMinutes(30))
            throw new BusinessRuleException("Reservation duration must be at least 30 minutes.");

        if (duration > TimeSpan.FromHours(8))
            throw new BusinessRuleException("Reservation duration must not exceed 8 hours.");

        var timeZone = ResolveTimeZone(space.TimeZoneId);

        var localStart = TimeZoneInfo.ConvertTime(startTime, timeZone);
        var localEnd = TimeZoneInfo.ConvertTime(endTime, timeZone);

        if (localStart.Date != localEnd.Date)
            throw new BusinessRuleException("Reservation must start and end on the same local day.");

        var start = TimeOnly.FromDateTime(localStart.DateTime);
        var end = TimeOnly.FromDateTime(localEnd.DateTime);

        if (start < space.OpeningTime || end > space.ClosingTime)
        {
            throw new BusinessRuleException(
                $"Reservation must be within space opening hours: {space.OpeningTime:HH:mm} - {space.ClosingTime:HH:mm}.");
        }
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException) when (timeZoneId == "America/Lima")
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
        catch (InvalidTimeZoneException) when (timeZoneId == "America/Lima")
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
    }

    private static ReservationDto ToDto(Reservation reservation)
    {
        return new ReservationDto(
            reservation.Id,
            reservation.ReservationCode,
            reservation.SpaceId,
            reservation.CustomerId,
            reservation.StartTime,
            reservation.EndTime,
            reservation.Status,
            reservation.BaseAmount,
            reservation.FinalAmount,
            reservation.RefundAmount,
            reservation.PricingBreakdown,
            reservation.CreatedAt,
            reservation.UpdatedAt,
            reservation.CancelledAt,
            reservation.CompletedAt);
    }
}