using Clinic.Domain.Tenants;
using MediatR;

namespace Clinic.Application.Tenants.Commands;

public sealed class CreateTenantCommandHandler(ITenantRepository tenants)
    : IRequestHandler<CreateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        if (await tenants.SlugExistsAsync(request.Slug, cancellationToken))
        {
            throw new InvalidOperationException("A tenant with this slug already exists.");
        }

        var tenant = new Tenant(request.Name, request.Slug, request.Status, request.SettingsJson);
        await tenants.AddAsync(tenant, cancellationToken);

        return new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.Status, tenant.SettingsJson);
    }
}
