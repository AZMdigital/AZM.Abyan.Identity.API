using AZM.Abyan.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AZM.Abyan.Identity.Persistence.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);

        // Required Properties
        builder.Property(x => x.LicenseKeyHash)
            .IsRequired();

        builder.Property(x => x.PackageName)
            .IsRequired()
            .HasMaxLength(100);

        // Optional Properties
        builder.Property(x => x.Domain)
            .HasMaxLength(500);

        builder.Property(x => x.ServerIps)
            .HasMaxLength(500);

        // Boolean status
        builder.Property(x => x.IsActive)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.Tenant)
            .WithMany() // Or WithMany(t => t.Licenses) if Tenant has a collection
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Client)
            .WithMany() // Or WithMany(c => c.Licenses) if Client has a collection
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        // Global Query Filter for soft delete
        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
