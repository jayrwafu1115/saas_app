using Clinic.Application.Billing;
using Clinic.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence.Repositories;

public sealed class BillingRepository(ApplicationDbContext dbContext) : IBillingRepository
{
    public async Task<IReadOnlyList<SubscriptionPlan>> ListPlansAsync(CancellationToken cancellationToken) =>
        await dbContext.SubscriptionPlans.AsNoTracking().Where(plan => plan.IsActive).OrderBy(plan => plan.MonthlyPricePhp).ToListAsync(cancellationToken);

    public Task<SubscriptionPlan?> GetPlanByCodeAsync(string code, CancellationToken cancellationToken) =>
        dbContext.SubscriptionPlans.FirstOrDefaultAsync(plan => plan.Code == code.Trim().ToLowerInvariant(), cancellationToken);

    public Task<SubscriptionPlan?> GetPlanByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.SubscriptionPlans.FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);

    public Task<TenantSubscription?> GetSubscriptionByTenantAsync(Guid tenantId, CancellationToken cancellationToken) =>
        dbContext.TenantSubscriptions.FirstOrDefaultAsync(subscription => subscription.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<TenantSubscription>> ListSubscriptionsAsync(CancellationToken cancellationToken) =>
        await dbContext.TenantSubscriptions.AsNoTracking().OrderBy(subscription => subscription.TenantId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SubscriptionUsage>> ListUsageAsync(Guid? tenantId, DateOnly period, CancellationToken cancellationToken)
    {
        var query = dbContext.SubscriptionUsages.AsNoTracking().Where(usage => usage.Period == period);
        if (tenantId.HasValue)
        {
            query = query.Where(usage => usage.TenantId == tenantId.Value);
        }

        return await query.OrderBy(usage => usage.Metric).ToListAsync(cancellationToken);
    }

    public async Task AddSubscriptionAsync(TenantSubscription subscription, CancellationToken cancellationToken)
    {
        dbContext.TenantSubscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddPaymentAsync(BillingPayment payment, CancellationToken cancellationToken)
    {
        dbContext.BillingPayments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertUsageAsync(Guid tenantId, string metric, int quantity, DateOnly period, CancellationToken cancellationToken)
    {
        var normalizedMetric = metric.Trim().ToLowerInvariant();
        var usage = await dbContext.SubscriptionUsages.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Metric == normalizedMetric && item.Period == period, cancellationToken);
        if (usage is null)
        {
            dbContext.SubscriptionUsages.Add(new SubscriptionUsage(tenantId, normalizedMetric, quantity, period));
        }
        else
        {
            usage.SetQuantity(quantity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
