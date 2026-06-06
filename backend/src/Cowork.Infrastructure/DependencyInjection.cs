using Cowork.Application.Common.Interfaces;
using Cowork.Infrastructure.Persistence;
using Cowork.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cowork.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }

        services.AddDbContext<CoworkDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<ISpaceRepository, SpaceRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}