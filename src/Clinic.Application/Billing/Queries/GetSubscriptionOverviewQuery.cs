using MediatR;

namespace Clinic.Application.Billing.Queries;

public sealed record GetSubscriptionOverviewQuery(Guid? TenantId) : IRequest<SubscriptionOverviewDto>;
