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

        var billableReservations = reservations
            .Where(x => x.Status != ReservationStatus.Cancelled)
            .ToList();

        var totalRevenue = billableReservations
            .Sum(x => x.FinalAmount);

        var totalRefundAmount = reservations
            .Where(x => x.RefundAmount.HasValue)
            .Sum(x => x.RefundAmount!.Value);

        var spaceReportItems = spaces
            .Select(space =>
            {
                var reservationsForSpace = reservations
                    .Where(x => x.SpaceId == space.Id)
                    .ToList();

                var billableReservationsForSpace = reservationsForSpace
                    .Where(x => x.Status != ReservationStatus.Cancelled)
                    .ToList();

                var revenue = billableReservationsForSpace
                    .Sum(x => x.FinalAmount);

                var occupancyRatePercent = CalculateOccupancyRatePercent(
                    billableReservationsForSpace,
                    space,
                    dateFrom,
                    dateTo);

                return new SpaceReportItemDto(
                    space.Id,
                    space.Name,
                    reservationsForSpace.Count,
                    revenue,
                    occupancyRatePercent);
            })
            .OrderByDescending(x => x.ReservationCount)
            .ThenByDescending(x => x.Revenue)
            .ThenBy(x => x.SpaceName)
            .ToList();

        var mostReservedSpaceName = spaceReportItems
            .Where(x => x.ReservationCount > 0)
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

    private static decimal CalculateOccupancyRatePercent(
        IReadOnlyList<Reservation> reservations,
        Space space,
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo)
    {
        var availableMinutes = CalculateAvailableMinutes(
            space,
            dateFrom,
            dateTo);

        if (availableMinutes <= 0)
            return 0;

        var occupiedMinutes = reservations.Sum(reservation =>
            CalculateOverlappedMinutes(
                reservation.StartTime,
                reservation.EndTime,
                dateFrom,
                dateTo));

        if (occupiedMinutes <= 0)
            return 0;

        var rate = (decimal)occupiedMinutes / availableMinutes * 100;

        return Math.Round(rate, 2);
    }

    private static decimal CalculateAvailableMinutes(
        Space space,
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo)
    {
        var timeZone = ResolveTimeZone(space.TimeZoneId);

        var localFrom = TimeZoneInfo.ConvertTime(dateFrom, timeZone).Date;
        var localTo = TimeZoneInfo.ConvertTime(dateTo, timeZone).Date;

        var days = (localTo - localFrom).Days + 1;

        if (days <= 0)
            return 0;

        var dailyAvailableMinutes = (decimal)(space.ClosingTime - space.OpeningTime).TotalMinutes;

        if (dailyAvailableMinutes <= 0)
            return 0;

        return days * dailyAvailableMinutes;
    }

    private static double CalculateOverlappedMinutes(
        DateTimeOffset reservationStart,
        DateTimeOffset reservationEnd,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd)
    {
        var effectiveStart = reservationStart > rangeStart
            ? reservationStart
            : rangeStart;

        var effectiveEnd = reservationEnd < rangeEnd
            ? reservationEnd
            : rangeEnd;

        if (effectiveStart >= effectiveEnd)
            return 0;

        return (effectiveEnd - effectiveStart).TotalMinutes;
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