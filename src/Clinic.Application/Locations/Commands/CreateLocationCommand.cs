using MediatR;

namespace Clinic.Application.Locations.Commands;

public sealed record CreateLocationCommand(Guid TenantId, string Name, string Code, string Address, string Phone)
    : IRequest<LocationDto>;
