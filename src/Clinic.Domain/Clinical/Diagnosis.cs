using Clinic.Domain.Common;

namespace Clinic.Domain.Clinical;

public sealed class Diagnosis : BaseEntity
{
    private Diagnosis()
    {
        Code = string.Empty;
        Description = string.Empty;
        Type = string.Empty;
    }

    public Diagnosis(Guid tenantId, Guid encounterId, string code, string description, string type)
    {
        TenantId = tenantId;
        EncounterId = encounterId;
        Code = code.Trim().ToUpperInvariant();
        Description = description.Trim();
        Type = type.Trim();
    }

    public Guid TenantId { get; private set; }
    public Guid EncounterId { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public string Type { get; private set; }
}
