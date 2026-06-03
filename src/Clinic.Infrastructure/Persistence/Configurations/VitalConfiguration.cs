using Clinic.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class VitalConfiguration : IEntityTypeConfiguration<Vital>
{
    public void Configure(EntityTypeBuilder<Vital> builder)
    {
        builder.ToTable("Vitals");
        builder.HasKey(vital => vital.Id);
        builder.Property(vital => vital.TemperatureCelsius).HasPrecision(5, 2);
        builder.Property(vital => vital.HeightCm).HasPrecision(6, 2);
        builder.Property(vital => vital.WeightKg).HasPrecision(6, 2);
        builder.Property(vital => vital.Notes).HasMaxLength(1000).IsRequired();
        builder.HasQueryFilter(vital => !vital.IsDeleted);
        builder.HasIndex(vital => new { vital.TenantId, vital.EncounterId, vital.RecordedAtUtc });
    }
}
