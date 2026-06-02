using MediatR;

namespace Clinic.Application.Locations.Queries;

public sealed record GetLocationsQuery(Guid? TenantId) : IRequest<IReadOnlyList<LocationDto>>;
