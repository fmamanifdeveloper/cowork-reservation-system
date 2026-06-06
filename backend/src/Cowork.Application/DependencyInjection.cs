using Cowork.Application.Cancellations;
using Cowork.Application.Pricing;
using Cowork.Application.Reservations;
using Cowork.Application.Spaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cowork.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<DynamicPricingCalculator>();
        services.AddScoped<CancellationPolicyService>();

        services.AddScoped<SpaceService>();
        services.AddScoped<PricingPreviewService>();
        services.AddScoped<ReservationService>();

        return services;
    }
}