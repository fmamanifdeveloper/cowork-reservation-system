namespace Cowork.Application.Public;

public sealed record PublicAvailabilityResponse(
    Guid SpaceId,
    string SpaceName,
    DateOnly Date,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    string TimeZoneId,
    IReadOnlyList<PublicAvailabilitySlotDto> Slots,
    IReadOnlyList<PublicReservedSlotDto> ReservedSlots);

public sealed record PublicAvailabilitySlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable);

public sealed record PublicReservedSlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime);