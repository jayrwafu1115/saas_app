using Clinic.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionUsageConfiguration : IEntityTypeConfiguration<SubscriptionUsage>
{
    public void Configure(EntityTypeBuilder<SubscriptionUsage> builder)
    {
        builder.ToTable("SubscriptionUsages");
        builder.HasKey(usage => usage.Id);
        builder.Property(usage => usage.Metric).HasMaxLength(80).IsRequired();
        builder.HasIndex(usage => new { usage.TenantId, usage.Metric, usage.Period }).IsUnique();
        builder.HasQueryFilter(usage => !usage.IsDeleted);
    }
}
