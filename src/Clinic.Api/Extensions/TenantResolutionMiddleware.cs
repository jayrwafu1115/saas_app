using Clinic.Application.Common.Interfaces;

namespace Clinic.Api.Extensions;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        ITenantResolver tenantResolver,
        ITenantContextService tenantContext)
    {
        var hasTenantHeader = httpContext.Request.Headers.ContainsKey("X-Tenant-Id")
            || httpContext.Request.Headers.ContainsKey("X-Tenant-Slug");

        if (hasTenantHeader)
        {
            var resolvedTenant = await tenantResolver.ResolveAsync(httpContext, httpContext.RequestAborted);
            if (resolvedTenant is null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsJsonAsync(new { message = "Tenant was not found." });
                return;
            }

            tenantContext.SetTenant(resolvedTenant.Id, resolvedTenant.Slug);
        }

        await next(httpContext);
    }
}
