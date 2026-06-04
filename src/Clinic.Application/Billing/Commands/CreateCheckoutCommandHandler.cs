using MediatR;

namespace Clinic.Application.Billing.Commands;

public sealed class CreateCheckoutCommandHandler(IBillingService billing) : IRequestHandler<CreateCheckoutCommand, BillingCheckoutDto>
{
    public Task<BillingCheckoutDto> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken) =>
        billing.CreateCheckoutAsync(request.TenantId, request.PlanCode, request.Provider, cancellationToken);
}
