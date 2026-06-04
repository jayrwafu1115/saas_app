using Clinic.Application.Common.Security;
using Clinic.Domain.Security;
using Clinic.Infrastructure.Persistence;

namespace Clinic.Infrastructure.Security;

public sealed class SecurityAuditService(ApplicationDbContext dbContext) : ISecurityAuditService
{
    public async Task RecordAsync(SecurityAuditEntry entry, CancellationToken cancellationToken = default)
    {
        dbContext.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            TenantId = entry.TenantId,
            UserId = entry.UserId,
            EventType = entry.EventType.Trim(),
            Subject = entry.Subject.Trim(),
            IpAddress = entry.IpAddress.Trim(),
            UserAgent = entry.UserAgent.Trim(),
            DetailsJson = string.IsNullOrWhiteSpace(entry.DetailsJson) ? "{}" : entry.DetailsJson
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
