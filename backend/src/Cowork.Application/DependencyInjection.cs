using Cowork.Application.Cancellations;
using Cowork.Application.Common.Interfaces;
using Cowork.Application.Common.Services;
using Cowork.Application.Customers;
using Cowork.Application.Pricing;
using Cowork.Application.Reports;
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

        services.AddScoped<IReservationCodeGenerator, ReservationCodeGenerator>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        services.AddScoped<CustomerService>();
        services.AddScoped<SpaceService>();
        services.AddScoped<PricingPreviewService>();
        services.AddScoped<ReservationService>();
        services.AddScoped<ReportsService>();

        return services;
    }
}