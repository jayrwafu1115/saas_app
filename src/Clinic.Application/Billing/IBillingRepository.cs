using Clinic.Domain.Billing;

namespace Clinic.Application.Billing;

public interface IBillingRepository
{
    Task<IReadOnlyList<SubscriptionPlan>> ListPlansAsync(CancellationToken cancellationToken);
    Task<SubscriptionPlan?> GetPlanByCodeAsync(string code, CancellationToken cancellationToken);
    Task<SubscriptionPlan?> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TenantSubscription?> GetSubscriptionByTenantAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantSubscription>> ListSubscriptionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<SubscriptionUsage>> ListUsageAsync(Guid? tenantId, DateOnly period, CancellationToken cancellationToken);
    Task AddSubscriptionAsync(TenantSubscription subscription, CancellationToken cancellationToken);
    Task AddPaymentAsync(BillingPayment payment, CancellationToken cancellationToken);
    Task UpsertUsageAsync(Guid tenantId, string metric, int quantity, DateOnly period, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
