namespace Cowork.Application.Reservations;

public sealed record CreateReservationRequest(
    Guid SpaceId,
    Guid CustomerId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);