using Clinic.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(appointment => appointment.Id);

        builder.Property(appointment => appointment.Reason)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(appointment => appointment.Notes)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(appointment => appointment.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.HasOne<Clinic.Domain.Tenants.Tenant>()
            .WithMany()
            .HasForeignKey(appointment => appointment.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Clinic.Domain.Tenants.Location>()
            .WithMany()
            .HasForeignKey(appointment => appointment.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Clinic.Domain.Patients.Patient>()
            .WithMany()
            .HasForeignKey(appointment => appointment.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(appointment => new { appointment.TenantId, appointment.StartsAtUtc });
        builder.HasIndex(appointment => new { appointment.TenantId, appointment.Status, appointment.StartsAtUtc });
        builder.HasIndex(appointment => new { appointment.DoctorUserId, appointment.StartsAtUtc, appointment.EndsAtUtc });
        builder.HasIndex(appointment => new { appointment.LocationId, appointment.StartsAtUtc, appointment.EndsAtUtc });
        builder.HasQueryFilter(appointment => !appointment.IsDeleted);
    }
}
