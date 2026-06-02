using Clinic.Application.Tenants;

namespace Clinic.Api.Extensions;

public sealed class HeaderTenantResolver(ITenantRepository tenants) : ITenantResolver
{
    public async Task<ResolvedTenant?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var tenantIdHeader = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
            return tenant is null ? null : new ResolvedTenant(tenant.Id, tenant.Slug);
        }

        var slug = httpContext.Request.Headers["X-Tenant-Slug"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var tenant = await tenants.GetBySlugAsync(slug, cancellationToken);
            return tenant is null ? null : new ResolvedTenant(tenant.Id, tenant.Slug);
        }

        return null;
    }
}
