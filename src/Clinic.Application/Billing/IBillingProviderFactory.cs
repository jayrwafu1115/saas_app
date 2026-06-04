using Clinic.Domain.Billing;

namespace Clinic.Application.Billing;

public interface IBillingProviderFactory
{
    IBillingProvider GetProvider(BillingProvider provider);
}
