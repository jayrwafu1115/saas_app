using MediatR;

namespace Clinic.Application.Billing.Queries;

public sealed class GetSubscriptionOverviewQueryHandler(IBillingService billing) : IRequestHandler<GetSubscriptionOverviewQuery, SubscriptionOverviewDto>
{
    public Task<SubscriptionOverviewDto> Handle(GetSubscriptionOverviewQuery request, CancellationToken cancellationToken) =>
        billing.GetOverviewAsync(request.TenantId, cancellationToken);
}
