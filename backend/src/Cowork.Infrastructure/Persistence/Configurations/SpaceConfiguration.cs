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

        builder.Property(x => x.Id).HasColumnName("id");

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

        builder.Property(x => x.TimeZoneId)
            .HasColumnName("time_zone_id")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status_id")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");

        builder.Property(x => x.Version).HasColumnName("version").IsRequired();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.DeletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}