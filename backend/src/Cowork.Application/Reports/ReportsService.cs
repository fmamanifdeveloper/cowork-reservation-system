using Cowork.Application.Common.Exceptions;
using Cowork.Application.Common.Interfaces;
using Cowork.Domain.Entities;
using Cowork.Domain.Enums;

namespace Cowork.Application.Reports;

public sealed class ReportsService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISpaceRepository _spaceRepository;

    public ReportsService(
        IReservationRepository reservationRepository,
        ISpaceRepository spaceRepository)
    {
        _reservationRepository = reservationRepository;
        _spaceRepository = spaceRepository;
    }

    public async Task<ReportsDashboardDto> GetDashboardAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var dateTo = to ?? now;
        var dateFrom = from ?? now.AddDays(-30);

        if (dateFrom >= dateTo)
            throw new BusinessRuleException("Report start date must be earlier than end date.");

        var reservations = await _reservationRepository.ListByRangeAsync(
            dateFrom,
            dateTo,
            cancellationToken);

        var spaces = await _spaceRepository.ListAsync(cancellationToken);

        var spacesById = spaces.ToDictionary(x => x.Id);

        var totalReservations = reservations.Count;
        var pendingReservations = reservations.Count(x => x.Status == ReservationStatus.Pending);
        var confirmedReservations = reservations.Count(x => x.Status == ReservationStatus.Confirmed);
        var cancelledReservations = reservations.Count(x => x.Status == ReservationStatus.Cancelled);
        var completedReservations = reservations.Count(x => x.Status == ReservationStatus.Completed);

        var totalRevenue = reservations
            .Where(x => x.Status != ReservationStatus.Cancelled)
            .Sum(x => x.FinalAmount);

        var totalRefundAmount = reservations
            .Where(x => x.RefundAmount.HasValue)
            .Sum(x => x.RefundAmount!.Value);

        var spaceReportItems = reservations
            .GroupBy(x => x.SpaceId)
            .Select(group =>
            {
                var spaceName = spacesById.TryGetValue(group.Key, out var space)
                    ? space.Name
                    : "Unknown space";

                var revenue = group
                    .Where(x => x.Status != ReservationStatus.Cancelled)
                    .Sum(x => x.FinalAmount);

                return new SpaceReportItemDto(
                    group.Key,
                    spaceName,
                    group.Count(),
                    revenue);
            })
            .OrderByDescending(x => x.ReservationCount)
            .ThenByDescending(x => x.Revenue)
            .ToList();

        var mostReservedSpaceName = spaceReportItems
            .FirstOrDefault()
            ?.SpaceName;

        var hourlyDemand = reservations
            .Select(reservation => new
            {
                Reservation = reservation,
                LocalHour = GetLocalStartHour(reservation, spacesById)
            })
            .GroupBy(x => x.LocalHour)
            .Select(group => new HourlyDemandDto(
                group.Key,
                group.Count()))
            .OrderBy(x => x.Hour)
            .ToList();

        var mostDemandedHour = hourlyDemand
            .OrderByDescending(x => x.ReservationCount)
            .ThenBy(x => x.Hour)
            .FirstOrDefault()
            ?.Hour;

        return new ReportsDashboardDto(
            dateFrom,
            dateTo,
            totalReservations,
            pendingReservations,
            confirmedReservations,
            cancelledReservations,
            completedReservations,
            totalRevenue,
            totalRefundAmount,
            mostReservedSpaceName,
            mostDemandedHour,
            spaceReportItems,
            hourlyDemand);
    }

    private static int GetLocalStartHour(
        Reservation reservation,
        IReadOnlyDictionary<Guid, Space> spacesById)
    {
        if (!spacesById.TryGetValue(reservation.SpaceId, out var space))
            return reservation.StartTime.UtcDateTime.Hour;

        var timeZone = ResolveTimeZone(space.TimeZoneId);
        var localStart = TimeZoneInfo.ConvertTime(reservation.StartTime, timeZone);

        return localStart.Hour;
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
}