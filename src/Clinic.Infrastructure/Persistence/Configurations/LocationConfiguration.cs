using Clinic.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(location => location.Id);

        builder.Property(location => location.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(location => location.Code)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(location => location.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(location => location.Phone)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasOne(location => location.Tenant)
            .WithMany()
            .HasForeignKey(location => location.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(location => new { location.TenantId, location.Code })
            .IsUnique();

        builder.HasQueryFilter(location => !location.IsDeleted);
    }
}
