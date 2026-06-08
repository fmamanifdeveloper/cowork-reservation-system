namespace Cowork.Application.Public;

public sealed record PublicCreateReservationRequest(
    Guid SpaceId,
    string CustomerFullName,
    string? CustomerEmail,
    string? CustomerPhone,
    string? CustomerDocumentNumber,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);