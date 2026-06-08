using Cowork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cowork.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ReservationCode)
            .HasColumnName("reservation_code")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.SpaceId)
            .HasColumnName("space_id")
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(x => x.CancelledByUserId).HasColumnName("cancelled_by_user_id");
        builder.Property(x => x.CompletedByUserId).HasColumnName("completed_by_user_id");

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status_id")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.BaseAmount)
            .HasColumnName("base_amount")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(x => x.FinalAmount)
            .HasColumnName("final_amount")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(x => x.RefundAmount)
            .HasColumnName("refund_amount")
            .HasColumnType("numeric(10,2)");

        builder.Property(x => x.PricingBreakdown)
            .HasColumnName("pricing_breakdown")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");

        builder.Property(x => x.Version).HasColumnName("version").IsRequired();

        builder.Ignore(x => x.DurationInHours);

        builder.HasIndex(x => x.ReservationCode).IsUnique();

        builder.HasOne<Space>()
            .WithMany()
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

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
            .HasForeignKey(x => x.CancelledByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}