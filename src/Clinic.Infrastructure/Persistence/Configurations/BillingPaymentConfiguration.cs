using Clinic.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class BillingPaymentConfiguration : IEntityTypeConfiguration<BillingPayment>
{
    public void Configure(EntityTypeBuilder<BillingPayment> builder)
    {
        builder.ToTable("BillingPayments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Provider).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(payment => payment.AmountPhp).HasPrecision(18, 2);
        builder.Property(payment => payment.ProviderReference).HasMaxLength(160).IsRequired();
        builder.Property(payment => payment.CheckoutUrl).HasMaxLength(1000).IsRequired();
        builder.HasIndex(payment => new { payment.TenantId, payment.CreatedAtUtc });
        builder.HasIndex(payment => new { payment.TenantId, payment.Status, payment.CreatedAtUtc });
        builder.HasQueryFilter(payment => !payment.IsDeleted);
    }
}
