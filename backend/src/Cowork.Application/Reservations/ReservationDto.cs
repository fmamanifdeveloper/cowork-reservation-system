using Cowork.Domain.Enums;

namespace Cowork.Application.Reservations;

public sealed record ReservationDto(
    Guid Id,
    Guid SpaceId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    ReservationStatus Status,
    decimal BaseAmount,
    decimal FinalAmount,
    decimal? RefundAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset? CompletedAt);