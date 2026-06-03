using Clinic.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class DiagnosisConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.ToTable("Diagnoses");
        builder.HasKey(diagnosis => diagnosis.Id);
        builder.Property(diagnosis => diagnosis.Code).HasMaxLength(40).IsRequired();
        builder.Property(diagnosis => diagnosis.Description).HasMaxLength(500).IsRequired();
        builder.Property(diagnosis => diagnosis.Type).HasMaxLength(80).IsRequired();
        builder.HasQueryFilter(diagnosis => !diagnosis.IsDeleted);
        builder.HasIndex(diagnosis => new { diagnosis.TenantId, diagnosis.EncounterId });
    }
}
