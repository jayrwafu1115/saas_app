using Clinic.Application.Tenants;
using Clinic.Domain.Tenants;
using MediatR;

namespace Clinic.Application.Locations.Commands;

public sealed class CreateLocationCommandHandler(
    ILocationRepository locations,
    ITenantRepository tenants)
    : IRequestHandler<CreateLocationCommand, LocationDto>
{
    public async Task<LocationDto> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            throw new InvalidOperationException("Tenant does not exist.");
        }

        if (await locations.CodeExistsAsync(request.TenantId, request.Code, cancellationToken))
        {
            throw new InvalidOperationException("A location with this code already exists for this tenant.");
        }

        var location = new Location(request.TenantId, request.Name, request.Code, request.Address, request.Phone);
        await locations.AddAsync(location, cancellationToken);

        return new LocationDto(location.Id, location.TenantId, location.Name, location.Code, location.Address, location.Phone);
    }
}
