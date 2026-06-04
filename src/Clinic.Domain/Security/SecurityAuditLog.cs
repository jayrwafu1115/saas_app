using Clinic.Domain.Common;

namespace Clinic.Domain.Security;

public sealed class SecurityAuditLog : BaseEntity
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string DetailsJson { get; set; } = "{}";
}
