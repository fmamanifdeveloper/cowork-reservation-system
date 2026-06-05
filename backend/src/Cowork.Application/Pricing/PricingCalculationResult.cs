namespace Cowork.Application.Pricing;

public sealed record PricingCalculationResult(
    decimal BaseAmount,
    decimal FinalAmount,
    IReadOnlyList<PricingAdjustment> Adjustments);