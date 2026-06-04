using Clinic.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("security_audit_logs");
        builder.HasKey(log => log.Id);

        builder.Property(log => log.EventType).HasMaxLength(120).IsRequired();
        builder.Property(log => log.Subject).HasMaxLength(250).IsRequired();
        builder.Property(log => log.IpAddress).HasMaxLength(80).IsRequired();
        builder.Property(log => log.UserAgent).HasMaxLength(500).IsRequired();
        builder.Property(log => log.DetailsJson).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(log => new { log.TenantId, log.CreatedAtUtc });
        builder.HasIndex(log => new { log.UserId, log.CreatedAtUtc });
        builder.HasIndex(log => new { log.EventType, log.CreatedAtUtc });
    }
}
