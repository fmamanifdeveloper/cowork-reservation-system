namespace Cowork.Application.Reports;

public sealed record ReportsDashboardDto(
    DateTimeOffset DateFrom,
    DateTimeOffset DateTo,
    int TotalReservations,
    int PendingReservations,
    int ConfirmedReservations,
    int CancelledReservations,
    int CompletedReservations,
    decimal TotalRevenue,
    decimal TotalRefundAmount,
    string? MostReservedSpaceName,
    int? MostDemandedHour,
    IReadOnlyList<SpaceReportItemDto> Spaces,
    IReadOnlyList<HourlyDemandDto> HourlyDemand);