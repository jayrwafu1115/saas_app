using Clinic.Domain.Common;

namespace Clinic.Domain.Billing;

public sealed class SubscriptionUsage : BaseEntity
{
    private SubscriptionUsage()
    {
        Metric = string.Empty;
    }

    public SubscriptionUsage(Guid tenantId, string metric, int quantity, DateOnly period)
    {
        TenantId = tenantId;
        Metric = metric.Trim().ToLowerInvariant();
        Quantity = quantity;
        Period = period;
    }

    public Guid TenantId { get; private set; }
    public string Metric { get; private set; }
    public int Quantity { get; private set; }
    public DateOnly Period { get; private set; }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
    }
}
