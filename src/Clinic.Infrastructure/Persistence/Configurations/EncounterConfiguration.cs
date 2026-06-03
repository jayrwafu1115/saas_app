using Clinic.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class EncounterConfiguration : IEntityTypeConfiguration<Encounter>
{
    public void Configure(EntityTypeBuilder<Encounter> builder)
    {
        builder.ToTable("Encounters");
        builder.HasKey(encounter => encounter.Id);
        builder.Property(encounter => encounter.ChiefComplaint).HasMaxLength(500).IsRequired();
        builder.Property(encounter => encounter.Subjective).HasMaxLength(4000).IsRequired();
        builder.Property(encounter => encounter.Objective).HasMaxLength(4000).IsRequired();
        builder.Property(encounter => encounter.Assessment).HasMaxLength(4000).IsRequired();
        builder.Property(encounter => encounter.Plan).HasMaxLength(4000).IsRequired();
        builder.Property(encounter => encounter.Notes).HasMaxLength(4000).IsRequired();
        builder.Property(encounter => encounter.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasQueryFilter(encounter => !encounter.IsDeleted);
        builder.HasIndex(encounter => new { encounter.TenantId, encounter.PatientId, encounter.EncounterDateUtc });
        builder.HasMany(encounter => encounter.Vitals).WithOne().HasForeignKey(vital => vital.EncounterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(encounter => encounter.Diagnoses).WithOne().HasForeignKey(diagnosis => diagnosis.EncounterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(encounter => encounter.Prescriptions).WithOne().HasForeignKey(prescription => prescription.EncounterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(encounter => encounter.AuditLogs).WithOne().HasForeignKey(log => log.EncounterId).OnDelete(DeleteBehavior.Cascade);
    }
}
