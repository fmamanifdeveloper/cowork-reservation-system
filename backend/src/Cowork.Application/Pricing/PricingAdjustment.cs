namespace Cowork.Application.Pricing;

public sealed record PricingAdjustment(
    string Rule,
    decimal Percentage,
    string Description,
    decimal AmountBefore,
    decimal AmountAfter);