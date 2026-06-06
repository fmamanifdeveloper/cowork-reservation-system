namespace Cowork.Application.Reports;

public sealed record SpaceOccupancyReportDto(
    Guid SpaceId,
    string SpaceName,
    decimal OccupancyRatePercentage,
    decimal Income);