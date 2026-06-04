using Clinic.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");
        builder.HasKey(subscription => subscription.Id);
        builder.Property(subscription => subscription.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(subscription => subscription.ProviderCustomerId).HasMaxLength(120).IsRequired();
        builder.HasIndex(subscription => subscription.TenantId).IsUnique();
        builder.HasQueryFilter(subscription => !subscription.IsDeleted);
    }
}
