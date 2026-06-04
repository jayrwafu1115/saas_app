using Clinic.Application.Billing;
using Clinic.Domain.Billing;

namespace Clinic.Infrastructure.Billing;

public sealed class BillingProviderFactory(IEnumerable<IBillingProvider> providers) : IBillingProviderFactory
{
    public IBillingProvider GetProvider(BillingProvider provider) =>
        providers.FirstOrDefault(candidate => candidate.Provider == provider)
        ?? throw new InvalidOperationException($"Billing provider '{provider}' is not configured.");
}
