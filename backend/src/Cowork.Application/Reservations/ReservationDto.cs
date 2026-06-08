using Cowork.Domain.Enums;

namespace Cowork.Application.Reservations;

public sealed record ReservationDto(
    Guid Id,
    string ReservationCode,
    Guid SpaceId,
    Guid CustomerId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    ReservationStatus Status,
    decimal BaseAmount,
    decimal FinalAmount,
    decimal? RefundAmount,
    string PricingBreakdown,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset? CompletedAt);