namespace Cowork.Application.Pricing;

public sealed record PricingPreviewRequest(
    Guid SpaceId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);