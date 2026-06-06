using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Application.Pricing;
using Cowork.Domain.Entities;

namespace Cowork.Application.Reservations;

public sealed class ReservationService
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DynamicPricingCalculator _pricingCalculator;

    public ReservationService(
        ISpaceRepository spaceRepository,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        DynamicPricingCalculator pricingCalculator)
    {
        _spaceRepository = spaceRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _pricingCalculator = pricingCalculator;
    }

    public async Task<IReadOnlyList<ReservationDto>> ListAsync(CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.ListAsync(cancellationToken);
        return reservations.Select(ToDto).ToList();
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var space = await _spaceRepository.GetByIdAsync(request.SpaceId, cancellationToken);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        if (!space.IsAvailableForReservation())
            throw new BusinessRuleException("The selected space is under maintenance and cannot receive reservations.");

        ValidateReservationSchedule(space, request.StartTime, request.EndTime);

        var createdAt = DateTimeOffset.UtcNow;

        var pricingResult = _pricingCalculator.Calculate(
            space,
            request.StartTime,
            request.EndTime,
            createdAt);

        var reservation = new Reservation(
            Guid.NewGuid(),
            space.Id,
            request.StartTime,
            request.EndTime,
            pricingResult.BaseAmount,
            pricingResult.FinalAmount,
            createdAt);

        _reservationRepository.Add(reservation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(reservation);
    }

    private static void ValidateReservationSchedule(
        Space space,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        if (startTime.Date != endTime.Date)
            throw new BusinessRuleException("Reservation must start and end on the same day.");

        var start = TimeOnly.FromTimeSpan(startTime.TimeOfDay);
        var end = TimeOnly.FromTimeSpan(endTime.TimeOfDay);

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
            reservation.SpaceId,
            reservation.StartTime,
            reservation.EndTime,
            reservation.Status,
            reservation.BaseAmount,
            reservation.FinalAmount,
            reservation.RefundAmount,
            reservation.CreatedAt);
    }
}