namespace Clinic.Application.Common.Security;

public interface ISecurityAuditService
{
    Task RecordAsync(SecurityAuditEntry entry, CancellationToken cancellationToken = default);
}

public sealed record SecurityAuditEntry(
    Guid? TenantId,
    Guid? UserId,
    string EventType,
    string Subject,
    string IpAddress,
    string UserAgent,
    string DetailsJson = "{}");
