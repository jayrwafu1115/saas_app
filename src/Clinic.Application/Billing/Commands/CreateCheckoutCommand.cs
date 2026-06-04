using Clinic.Domain.Billing;
using MediatR;

namespace Clinic.Application.Billing.Commands;

public sealed record CreateCheckoutCommand(Guid TenantId, string PlanCode, BillingProvider Provider) : IRequest<BillingCheckoutDto>;
