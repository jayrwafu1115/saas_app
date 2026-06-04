using MediatR;

namespace Clinic.Application.Billing.Queries;

public sealed record GetTenantRestrictionQuery(Guid TenantId) : IRequest<TenantRestrictionDto>;
