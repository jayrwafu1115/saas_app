using Clinic.Application.Common.Interfaces;

namespace Clinic.Api.Extensions;

public sealed class HeaderCurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public Guid? TenantId
    {
        get
        {
            var headerValue = httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            return Guid.TryParse(headerValue, out var tenantId) ? tenantId : null;
        }
    }
}
