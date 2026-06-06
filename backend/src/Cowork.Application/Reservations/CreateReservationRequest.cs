namespace Cowork.Application.Reservations;

public sealed record CreateReservationRequest(
    Guid SpaceId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);