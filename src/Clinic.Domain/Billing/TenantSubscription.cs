using Clinic.Domain.Common;

namespace Clinic.Domain.Billing;

public sealed class TenantSubscription : BaseEntity
{
    private TenantSubscription()
    {
        ProviderCustomerId = string.Empty;
    }

    public TenantSubscription(Guid tenantId, Guid planId, SubscriptionStatus status, DateTimeOffset startedAtUtc, DateTimeOffset trialEndsAtUtc)
    {
        TenantId = tenantId;
        PlanId = planId;
        Status = status;
        StartedAtUtc = startedAtUtc;
        TrialEndsAtUtc = trialEndsAtUtc;
        CurrentPeriodStartUtc = startedAtUtc;
        CurrentPeriodEndUtc = trialEndsAtUtc;
        ProviderCustomerId = string.Empty;
    }

    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset TrialEndsAtUtc { get; private set; }
    public DateTimeOffset CurrentPeriodStartUtc { get; private set; }
    public DateTimeOffset CurrentPeriodEndUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public string ProviderCustomerId { get; private set; }

    public void Activate(DateTimeOffset periodStartUtc, DateTimeOffset periodEndUtc)
    {
        Status = SubscriptionStatus.Active;
        CurrentPeriodStartUtc = periodStartUtc;
        CurrentPeriodEndUtc = periodEndUtc;
    }

    public void ChangePlan(Guid planId)
    {
        PlanId = planId;
    }

    public void Cancel(DateTimeOffset cancelledAtUtc)
    {
        Status = SubscriptionStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
    }

    public bool IsRestricted(DateTimeOffset nowUtc) =>
        Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired || (Status == SubscriptionStatus.Trialing && TrialEndsAtUtc < nowUtc) || Status == SubscriptionStatus.PastDue;
}
