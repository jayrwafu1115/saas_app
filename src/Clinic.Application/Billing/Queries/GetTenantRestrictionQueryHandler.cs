using MediatR;

namespace Clinic.Application.Billing.Queries;

public sealed class GetTenantRestrictionQueryHandler(IBillingService billing) : IRequestHandler<GetTenantRestrictionQuery, TenantRestrictionDto>
{
    public Task<TenantRestrictionDto> Handle(GetTenantRestrictionQuery request, CancellationToken cancellationToken) =>
        billing.GetRestrictionAsync(request.TenantId, cancellationToken);
}
