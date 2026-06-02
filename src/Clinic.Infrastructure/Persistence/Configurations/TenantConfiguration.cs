using Clinic.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(tenant => tenant.Id);

        builder.Property(tenant => tenant.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(tenant => tenant.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(tenant => tenant.Status)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(tenant => tenant.SettingsJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.HasIndex(tenant => tenant.Slug)
            .IsUnique();

        builder.HasQueryFilter(tenant => !tenant.IsDeleted);
    }
}
