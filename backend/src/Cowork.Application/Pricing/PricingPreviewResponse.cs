namespace Cowork.Application.Pricing;

public sealed record PricingPreviewResponse(
    decimal BaseAmount,
    decimal FinalAmount,
    IReadOnlyList<PricingAdjustment> Adjustments);