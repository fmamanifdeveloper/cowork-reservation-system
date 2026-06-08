using Cowork.Domain.Enums;

namespace Cowork.Application.Spaces;

public sealed record SpaceDto(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    string TimeZoneId,
    SpaceStatus Status);