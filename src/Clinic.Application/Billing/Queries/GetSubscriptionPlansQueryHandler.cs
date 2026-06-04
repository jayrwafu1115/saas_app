using MediatR;

namespace Clinic.Application.Billing.Queries;

public sealed class GetSubscriptionPlansQueryHandler(IBillingService billing) : IRequestHandler<GetSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanDto>>
{
    public Task<IReadOnlyList<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken) =>
        billing.ListPlansAsync(cancellationToken);
}
