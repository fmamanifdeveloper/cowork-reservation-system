using Cowork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cowork.Infrastructure.Persistence.Configurations;

public sealed class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.ToTable("spaces");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Capacity)
            .HasColumnName("capacity")
            .IsRequired();

        builder.Property(x => x.BaseHourlyRate)
            .HasColumnName("base_hourly_rate")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(x => x.OpeningTime)
            .HasColumnName("opening_time")
            .HasColumnType("time")
            .IsRequired();

        builder.Property(x => x.ClosingTime)
            .HasColumnName("closing_time")
            .HasColumnType("time")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();
    }
}