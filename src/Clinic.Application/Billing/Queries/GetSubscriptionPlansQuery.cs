using MediatR;

namespace Clinic.Application.Billing.Queries;

public sealed record GetSubscriptionPlansQuery() : IRequest<IReadOnlyList<SubscriptionPlanDto>>;
