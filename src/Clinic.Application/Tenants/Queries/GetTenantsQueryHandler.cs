using MediatR;

namespace Clinic.Application.Tenants.Queries;

public sealed class GetTenantsQueryHandler(ITenantRepository tenants)
    : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    public async Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var results = await tenants.ListAsync(cancellationToken);
        return results
            .Select(tenant => new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.Status, tenant.SettingsJson))
            .ToList();
    }
}
