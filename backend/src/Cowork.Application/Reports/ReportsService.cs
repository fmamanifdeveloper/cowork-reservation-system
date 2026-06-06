using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Enums;

namespace Cowork.Application.Reports;

public sealed class ReportsService
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IReservationRepository _reservationRepository;

    public ReportsService(
        ISpaceRepository spaceRepository,
        IReservationRepository reservationRepository)
    {
        _spaceRepository = spaceRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<ReportsResponse> GetAsync(
        ReportsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.From >= request.To)
            throw new BusinessRuleException("Report start date must be earlier than end date.");

        var spaces = await _spaceRepository.ListAsync(cancellationToken);

        var reservations = await _reservationRepository.ListByRangeAsync(
            request.From,
            request.To,
            cancellationToken);

        var effectiveReservations = reservations
            .Where(x => x.Status is ReservationStatus.Confirmed or ReservationStatus.Completed)
            .ToList();

        var totalRangeHours = (decimal)(request.To - request.From).TotalHours;

        var spaceReports = spaces
            .Select(space =>
            {
                var reservationsBySpace = effectiveReservations
                    .Where(x => x.SpaceId == space.Id)
                    .ToList();

                var reservedHours = reservationsBySpace
                    .Sum(x => (decimal)(x.EndTime - x.StartTime).TotalHours);

                var occupancy = totalRangeHours <= 0
                    ? 0
                    : Math.Round((reservedHours / totalRangeHours) * 100, 2, MidpointRounding.AwayFromZero);

                var income = reservationsBySpace
                    .Sum(x => x.FinalAmount);

                return new SpaceOccupancyReportDto(
                    space.Id,
                    space.Name,
                    occupancy,
                    income);
            })
            .ToList();

        var totalIncome = spaceReports.Sum(x => x.Income);

        var mostDemandedHour = effectiveReservations
            .GroupBy(x => x.StartTime.Hour)
            .OrderByDescending(x => x.Count())
            .Select(x => $"{x.Key:00}:00")
            .FirstOrDefault();

        return new ReportsResponse(
            request.From,
            request.To,
            totalIncome,
            mostDemandedHour,
            spaceReports);
    }
}