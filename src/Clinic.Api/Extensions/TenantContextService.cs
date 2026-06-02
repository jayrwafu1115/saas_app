using Clinic.Application.Common.Interfaces;

namespace Clinic.Api.Extensions;

public sealed class TenantContextService : ITenantContextService
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }

    public void SetTenant(Guid tenantId, string tenantSlug)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
    }
}
