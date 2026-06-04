using Clinic.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");
        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.MedicalRecordNumber).HasMaxLength(60).IsRequired();
        builder.Property(patient => patient.FirstName).HasMaxLength(120).IsRequired();
        builder.Property(patient => patient.MiddleName).HasMaxLength(120).IsRequired();
        builder.Property(patient => patient.LastName).HasMaxLength(120).IsRequired();
        builder.Property(patient => patient.Gender).HasMaxLength(40).IsRequired();
        builder.Property(patient => patient.Email).HasMaxLength(200).IsRequired();
        builder.Property(patient => patient.Phone).HasMaxLength(40).IsRequired();
        builder.Property(patient => patient.Address).HasMaxLength(500).IsRequired();

        builder.HasIndex(patient => new { patient.TenantId, patient.MedicalRecordNumber })
            .IsUnique();
        builder.HasIndex(patient => new { patient.TenantId, patient.LastName, patient.FirstName });
        builder.HasIndex(patient => new { patient.TenantId, patient.CreatedAtUtc });

        builder.HasOne<Clinic.Domain.Tenants.Tenant>()
            .WithMany()
            .HasForeignKey(patient => patient.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Clinic.Domain.Tenants.Location>()
            .WithMany()
            .HasForeignKey(patient => patient.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(patient => !patient.IsDeleted);
    }
}
