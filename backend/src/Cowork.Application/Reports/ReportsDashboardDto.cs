namespace Cowork.Application.Reports;

public sealed record ReportsDashboardDto(
    DateTimeOffset From,
    DateTimeOffset To,
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