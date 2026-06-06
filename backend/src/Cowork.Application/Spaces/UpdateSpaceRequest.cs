using Cowork.Domain.Enums;

namespace Cowork.Application.Spaces;

public sealed record UpdateSpaceRequest(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    SpaceStatus Status);