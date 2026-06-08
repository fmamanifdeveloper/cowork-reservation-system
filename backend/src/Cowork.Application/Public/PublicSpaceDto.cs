using Cowork.Domain.Enums;

namespace Cowork.Application.Public;

public sealed record PublicSpaceDto(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    string TimeZoneId,
    SpaceStatus Status);