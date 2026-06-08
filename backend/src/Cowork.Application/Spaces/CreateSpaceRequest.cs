using Cowork.Domain.Enums;

namespace Cowork.Application.Spaces;

public sealed record CreateSpaceRequest(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    string? TimeZoneId,
    SpaceStatus Status);