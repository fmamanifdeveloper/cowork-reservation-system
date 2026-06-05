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

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.SpaceId)
            .HasColumnName("space_id")
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
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

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Ignore(x => x.DurationInHours);

        builder.HasOne<Space>()
            .WithMany()
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}