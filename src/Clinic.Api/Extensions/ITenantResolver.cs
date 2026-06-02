namespace Clinic.Api.Extensions;

public interface ITenantResolver
{
    Task<ResolvedTenant?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken);
}

public sealed record ResolvedTenant(Guid Id, string Slug);
