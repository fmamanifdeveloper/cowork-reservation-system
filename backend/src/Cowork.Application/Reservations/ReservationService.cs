using Cowork.Application.Cancellations;
using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Application.Pricing;
using Cowork.Domain.Entities;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly DynamicPricingCalculator _pricingCalculator;
    private readonly CancellationPolicyService _cancellationPolicyService;

    public ReservationService(
        ISpaceRepository spaceRepository,
        ICustomerRepository customerRepository,
        IReservationRepository reservationRepository,
        IReservationCodeGenerator reservationCodeGenerator,
        IAuditLogger auditLogger,
        IUnitOfWork unitOfWork,
        DynamicPricingCalculator pricingCalculator,
        CancellationPolicyService cancellationPolicyService)
    {
        _spaceRepository = spaceRepository;
        _customerRepository = customerRepository;
        _reservationRepository = reservationRepository;
        _reservationCodeGenerator = reservationCodeGenerator;
        _auditLogger = auditLogger;
        _unitOfWork = unitOfWork;
        _pricingCalculator = pricingCalculator;
        _cancellationPolicyService = cancellationPolicyService;
    }

    public async Task<IReadOnlyList<ReservationDto>> ListAsync(CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.ListAsync(cancellationToken);
        return reservations.Select(ToDto).ToList();
    }

    public async Task<ReservationDto> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var space = await _spaceRepository.GetByIdAsync(request.SpaceId, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        if (!space.IsAvailableForReservation())
            throw new BusinessRuleException("The selected space is not available for reservations.");

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer was not found.");

        ValidateReservationSchedule(
            space,
            request.StartTime,
            request.EndTime);

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
            null,
            request.StartTime.ToUniversalTime(),
            request.EndTime.ToUniversalTime(),
            pricingResult.BaseAmount,
            pricingResult.FinalAmount,
            pricingBreakdown,
            createdAt);

        _reservationRepository.Add(reservation);

        await _auditLogger.LogAsync(
            "ReservationCreated",
            "Reservation",
            reservation.Id,
            null,
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
        var reservation = await _reservationRepository.GetByIdAsync(id, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation was not found.");

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
            cancellationRequestedAt);

        await _auditLogger.LogAsync(
            "ReservationCancelled",
            "Reservation",
            reservation.Id,
            null,
            reservation.CustomerId,
            "Cancel",
            "Reservation was cancelled.",
            oldValues,
            new
            {
                reservation.Status,
                reservation.RefundAmount,
                reservation.CancelledAt
            },
            null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(reservation);
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
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(space.TimeZoneId);

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