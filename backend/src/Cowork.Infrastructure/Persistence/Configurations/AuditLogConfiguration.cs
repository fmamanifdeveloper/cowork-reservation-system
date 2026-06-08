using Cowork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cowork.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.EntityId).HasColumnName("entity_id");

        builder.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(x => x.ActorCustomerId).HasColumnName("actor_customer_id");

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(x => x.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(x => x.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(80);

        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(300);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.ActorCustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}