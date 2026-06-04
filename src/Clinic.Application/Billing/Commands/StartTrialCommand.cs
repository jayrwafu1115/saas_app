using MediatR;

namespace Clinic.Application.Billing.Commands;

public sealed record StartTrialCommand(Guid TenantId, string PlanCode) : IRequest<TenantSubscriptionDto>;
