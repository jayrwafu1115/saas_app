using Clinic.Application.Billing;
using Clinic.Domain.Billing;
using Microsoft.Extensions.Options;

namespace Clinic.Infrastructure.Billing;

public sealed class GCashBillingProvider(IOptions<PhilippinesBillingOptions> options) : IBillingProvider
{
    public BillingProvider Provider => BillingProvider.GCash;

    public Task<BillingProviderCheckout> CreateCheckoutAsync(Guid tenantId, string planName, decimal amountPhp, CancellationToken cancellationToken)
    {
        var reference = $"gcash-{Guid.NewGuid():N}";
        var url = $"{options.Value.CheckoutBaseUrl}/gcash?tenantId={tenantId}&reference={reference}&amount={amountPhp:0.00}";
        return Task.FromResult(new BillingProviderCheckout(reference, url));
    }
}
