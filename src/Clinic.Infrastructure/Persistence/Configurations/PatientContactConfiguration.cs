using Clinic.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class PatientContactConfiguration : IEntityTypeConfiguration<PatientContact>
{
    public void Configure(EntityTypeBuilder<PatientContact> builder)
    {
        builder.ToTable("patient_contacts");
        builder.HasKey(contact => contact.Id);

        builder.Property(contact => contact.Name).HasMaxLength(200).IsRequired();
        builder.Property(contact => contact.Relationship).HasMaxLength(80).IsRequired();
        builder.Property(contact => contact.Email).HasMaxLength(200).IsRequired();
        builder.Property(contact => contact.Phone).HasMaxLength(40).IsRequired();

        builder.HasOne(contact => contact.Patient)
            .WithMany(patient => patient.Contacts)
            .HasForeignKey(contact => contact.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(contact => !contact.IsDeleted);
    }
}
