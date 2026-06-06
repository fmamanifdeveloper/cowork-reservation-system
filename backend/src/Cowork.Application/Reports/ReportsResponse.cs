namespace Cowork.Application.Reports;

public sealed record ReportsResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    decimal TotalIncome,
    string? MostDemandedHour,
    IReadOnlyList<SpaceOccupancyReportDto> Spaces);