using MediatR;

namespace Clinic.Application.Tenants.Queries;

public sealed record GetTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;
