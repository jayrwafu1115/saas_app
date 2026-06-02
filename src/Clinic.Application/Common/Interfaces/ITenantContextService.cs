namespace Clinic.Application.Common.Interfaces;

public interface ITenantContextService : ICurrentTenant
{
    string? TenantSlug { get; }
    void SetTenant(Guid tenantId, string tenantSlug);
}
