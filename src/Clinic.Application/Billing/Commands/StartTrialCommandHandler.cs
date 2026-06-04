using MediatR;

namespace Clinic.Application.Billing.Commands;

public sealed class StartTrialCommandHandler(IBillingService billing) : IRequestHandler<StartTrialCommand, TenantSubscriptionDto>
{
    public Task<TenantSubscriptionDto> Handle(StartTrialCommand request, CancellationToken cancellationToken) =>
        billing.StartTrialAsync(request.TenantId, request.PlanCode, cancellationToken);
}
