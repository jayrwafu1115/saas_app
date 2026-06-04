using Clinic.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(plan => plan.Id);
        builder.Property(plan => plan.Name).HasMaxLength(120).IsRequired();
        builder.Property(plan => plan.Code).HasMaxLength(80).IsRequired();
        builder.Property(plan => plan.MonthlyPricePhp).HasPrecision(18, 2);
        builder.Property(plan => plan.FeaturesJson).HasColumnType("jsonb").HasDefaultValue("{}").IsRequired();
        builder.HasIndex(plan => plan.Code).IsUnique();
        builder.HasQueryFilter(plan => !plan.IsDeleted);
    }
}
