using Clinic.Domain.Common;

namespace Clinic.Domain.Billing;

public sealed class BillingPayment : BaseEntity
{
    private BillingPayment()
    {
        ProviderReference = string.Empty;
        CheckoutUrl = string.Empty;
    }

    public BillingPayment(Guid tenantId, Guid subscriptionId, BillingProvider provider, decimal amountPhp, string providerReference, string checkoutUrl)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        Provider = provider;
        AmountPhp = amountPhp;
        ProviderReference = providerReference;
        CheckoutUrl = checkoutUrl;
        Status = PaymentStatus.Pending;
    }

    public Guid TenantId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public BillingProvider Provider { get; private set; }
    public decimal AmountPhp { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string ProviderReference { get; private set; }
    public string CheckoutUrl { get; private set; }

    public void MarkPaid()
    {
        Status = PaymentStatus.Paid;
    }
}
