using Cowork.Domain.Enums;

namespace Cowork.Application.Public;

public sealed record PublicReservationResponse(
    Guid ReservationId,
    string ReservationCode,
    Guid CustomerId,
    Guid SpaceId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    ReservationStatus Status,
    decimal BaseAmount,
    decimal FinalAmount,
    string PricingBreakdown);