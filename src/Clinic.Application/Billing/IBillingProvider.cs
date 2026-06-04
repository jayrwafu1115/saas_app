using Clinic.Domain.Billing;

namespace Clinic.Application.Billing;

public interface IBillingProvider
{
    BillingProvider Provider { get; }
    Task<BillingProviderCheckout> CreateCheckoutAsync(Guid tenantId, string planName, decimal amountPhp, CancellationToken cancellationToken);
}
