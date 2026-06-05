using Cowork.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cowork.Infrastructure.Persistence;

public sealed class CoworkDbContext : DbContext
{
    public CoworkDbContext(DbContextOptions<CoworkDbContext> options)
        : base(options)
    {
    }

    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoworkDbContext).Assembly);
    }
}