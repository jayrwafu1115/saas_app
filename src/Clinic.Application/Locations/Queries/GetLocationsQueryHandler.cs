using Clinic.Application.Common.Interfaces;
using MediatR;

namespace Clinic.Application.Locations.Queries;

public sealed class GetLocationsQueryHandler(ILocationRepository locations, ICurrentTenant currentTenant)
    : IRequestHandler<GetLocationsQuery, IReadOnlyList<LocationDto>>
{
    public async Task<IReadOnlyList<LocationDto>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId ?? currentTenant.TenantId;
        var results = await locations.ListAsync(tenantId, cancellationToken);

        return results
            .Select(location => new LocationDto(
                location.Id,
                location.TenantId,
                location.Name,
                location.Code,
                location.Address,
                location.Phone))
            .ToList();
    }
}
