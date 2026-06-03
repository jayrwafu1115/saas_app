using Clinic.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class EncounterAuditLogConfiguration : IEntityTypeConfiguration<EncounterAuditLog>
{
    public void Configure(EntityTypeBuilder<EncounterAuditLog> builder)
    {
        builder.ToTable("EncounterAuditLogs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Action).HasMaxLength(120).IsRequired();
        builder.Property(log => log.Summary).HasMaxLength(1000).IsRequired();
        builder.Property(log => log.ActorUserId).HasMaxLength(80).IsRequired();
        builder.HasQueryFilter(log => !log.IsDeleted);
        builder.HasIndex(log => new { log.TenantId, log.EncounterId, log.CreatedAtUtc });
    }
}
