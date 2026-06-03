using Clinic.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");
        builder.HasKey(prescription => prescription.Id);
        builder.Property(prescription => prescription.MedicationName).HasMaxLength(200).IsRequired();
        builder.Property(prescription => prescription.Dosage).HasMaxLength(120).IsRequired();
        builder.Property(prescription => prescription.Frequency).HasMaxLength(120).IsRequired();
        builder.Property(prescription => prescription.Duration).HasMaxLength(120).IsRequired();
        builder.Property(prescription => prescription.Instructions).HasMaxLength(1000).IsRequired();
        builder.HasQueryFilter(prescription => !prescription.IsDeleted);
        builder.HasIndex(prescription => new { prescription.TenantId, prescription.EncounterId });
    }
}
