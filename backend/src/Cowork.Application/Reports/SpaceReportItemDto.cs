namespace Cowork.Application.Reports;

public sealed record SpaceReportItemDto(
    Guid SpaceId,
    string SpaceName,
    int ReservationCount,
    decimal Revenue);