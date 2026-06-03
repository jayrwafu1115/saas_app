using Clinic.Domain.Common;

namespace Clinic.Domain.Clinical;

public sealed class EncounterAuditLog : BaseEntity
{
    private EncounterAuditLog()
    {
        Action = string.Empty;
        Summary = string.Empty;
        ActorUserId = string.Empty;
    }

    public EncounterAuditLog(Guid tenantId, Guid encounterId, string action, string summary, string? actorUserId)
    {
        TenantId = tenantId;
        EncounterId = encounterId;
        Action = action.Trim();
        Summary = summary.Trim();
        ActorUserId = actorUserId ?? string.Empty;
    }

    public Guid TenantId { get; private set; }
    public Guid EncounterId { get; private set; }
    public string Action { get; private set; }
    public string Summary { get; private set; }
    public string ActorUserId { get; private set; }
}
