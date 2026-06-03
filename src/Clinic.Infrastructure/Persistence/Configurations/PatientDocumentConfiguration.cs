using Clinic.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class PatientDocumentConfiguration : IEntityTypeConfiguration<PatientDocument>
{
    public void Configure(EntityTypeBuilder<PatientDocument> builder)
    {
        builder.ToTable("patient_documents");
        builder.HasKey(document => document.Id);

        builder.Property(document => document.FileName).HasMaxLength(255).IsRequired();
        builder.Property(document => document.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(document => document.ObjectKey).HasMaxLength(600).IsRequired();

        builder.HasIndex(document => document.ObjectKey).IsUnique();

        builder.HasOne(document => document.Patient)
            .WithMany(patient => patient.Documents)
            .HasForeignKey(document => document.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(document => !document.IsDeleted);
    }
}
