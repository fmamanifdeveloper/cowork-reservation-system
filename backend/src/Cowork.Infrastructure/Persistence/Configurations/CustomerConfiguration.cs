using Cowork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cowork.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(160);

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(40);

        builder.Property(x => x.DocumentNumber)
            .HasColumnName("document_number")
            .HasMaxLength(40);

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