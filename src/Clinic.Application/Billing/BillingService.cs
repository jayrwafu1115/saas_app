using Clinic.Application.Common.Interfaces;
using Clinic.Application.Locations;
using Clinic.Application.Patients;
using Clinic.Domain.Billing;

namespace Clinic.Application.Billing;

public sealed class BillingService(
    IBillingRepository billing,
    IBillingProviderFactory providers,
    ILocationRepository locations,
    IPatientRepository patients,
    IDateTimeProvider clock) : IBillingService
{
    public async Task<IReadOnlyList<SubscriptionPlanDto>> ListPlansAsync(CancellationToken cancellationToken) =>
        (await billing.ListPlansAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<TenantSubscriptionDto> StartTrialAsync(Guid tenantId, string planCode, CancellationToken cancellationToken)
    {
        var plan = await billing.GetPlanByCodeAsync(planCode, cancellationToken)
            ?? throw new KeyNotFoundException("Plan was not found.");
        var existing = await billing.GetSubscriptionByTenantAsync(tenantId, cancellationToken);
        if (existing is not null)
        {
            existing.ChangePlan(plan.Id);
            await billing.SaveChangesAsync(cancellationToken);
            return await ToDtoAsync(existing, cancellationToken);
        }

        var now = clock.UtcNow;
        var subscription = new TenantSubscription(tenantId, plan.Id, SubscriptionStatus.Trialing, now, now.AddDays(plan.TrialDays));
        await billing.AddSubscriptionAsync(subscription, cancellationToken);
        return await ToDtoAsync(subscription, cancellationToken);
    }

    public async Task<BillingCheckoutDto> CreateCheckoutAsync(Guid tenantId, string planCode, BillingProvider provider, CancellationToken cancellationToken)
    {
        var subscription = await billing.GetSubscriptionByTenantAsync(tenantId, cancellationToken)
            ?? await CreateDefaultTrialAsync(tenantId, planCode, cancellationToken);
        var plan = await billing.GetPlanByCodeAsync(planCode, cancellationToken)
            ?? throw new KeyNotFoundException("Plan was not found.");
        subscription.ChangePlan(plan.Id);
        var checkout = await providers.GetProvider(provider).CreateCheckoutAsync(tenantId, plan.Name, plan.MonthlyPricePhp, cancellationToken);
        var payment = new BillingPayment(tenantId, subscription.Id, provider, plan.MonthlyPricePhp, checkout.ProviderReference, checkout.CheckoutUrl);
        await billing.AddPaymentAsync(payment, cancellationToken);
        return new BillingCheckoutDto(payment.Id, provider, payment.AmountPhp, payment.CheckoutUrl, payment.ProviderReference);
    }

    public async Task<SubscriptionOverviewDto> GetOverviewAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var period = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var subscriptions = (await billing.ListSubscriptionsAsync(cancellationToken))
            .Where(subscription => !tenantId.HasValue || subscription.TenantId == tenantId.Value)
            .ToList();

        var usage = new List<SubscriptionUsageDto>();
        foreach (var subscription in subscriptions)
        {
            await RefreshUsageAsync(subscription.TenantId, period, cancellationToken);
            usage.AddRange(await GetUsageDtosAsync(subscription.TenantId, period, cancellationToken));
        }

        return new SubscriptionOverviewDto(
            (await Task.WhenAll(subscriptions.Select(subscription => ToDtoAsync(subscription, cancellationToken)))).ToList(),
            usage);
    }

    public async Task<TenantRestrictionDto> GetRestrictionAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var subscription = await billing.GetSubscriptionByTenantAsync(tenantId, cancellationToken);
        if (subscription is null)
        {
            return new TenantRestrictionDto(tenantId, true, "Tenant has no active subscription.");
        }

        var restriction = subscription.IsRestricted(clock.UtcNow);
        return new TenantRestrictionDto(tenantId, restriction, restriction ? $"Subscription is {subscription.Status}." : "Tenant is within subscription limits.");
    }

    private async Task<TenantSubscription> CreateDefaultTrialAsync(Guid tenantId, string planCode, CancellationToken cancellationToken)
    {
        var dto = await StartTrialAsync(tenantId, planCode, cancellationToken);
        return await billing.GetSubscriptionByTenantAsync(dto.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription could not be created.");
    }

    private async Task RefreshUsageAsync(Guid tenantId, DateOnly period, CancellationToken cancellationToken)
    {
        var locationsCount = (await locations.ListAsync(tenantId, cancellationToken)).Count;
        var patientsCount = (await patients.SearchAsync(tenantId, null, null, 1, 1, cancellationToken)).TotalCount;
        await billing.UpsertUsageAsync(tenantId, "locations", locationsCount, period, cancellationToken);
        await billing.UpsertUsageAsync(tenantId, "patients", patientsCount, period, cancellationToken);
    }

    private async Task<IReadOnlyList<SubscriptionUsageDto>> GetUsageDtosAsync(Guid tenantId, DateOnly period, CancellationToken cancellationToken)
    {
        var subscription = await billing.GetSubscriptionByTenantAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription was not found.");
        var plan = await billing.GetPlanByIdAsync(subscription.PlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan was not found.");
        var usages = await billing.ListUsageAsync(tenantId, period, cancellationToken);
        return usages.Select(usage =>
        {
            var limit = usage.Metric switch
            {
                "locations" => plan.MaxLocations,
                "patients" => plan.MaxPatients,
                "doctors" => plan.MaxDoctors,
                "users" => plan.MaxUsers,
                _ => 0
            };
            return new SubscriptionUsageDto(usage.TenantId, usage.Metric, usage.Quantity, limit, usage.Period, limit > 0 && usage.Quantity > limit);
        }).ToList();
    }

    private async Task<TenantSubscriptionDto> ToDtoAsync(TenantSubscription subscription, CancellationToken cancellationToken)
    {
        var plan = await billing.GetPlanByIdAsync(subscription.PlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan was not found.");
        return new TenantSubscriptionDto(subscription.Id, subscription.TenantId, plan.Id, plan.Name, subscription.Status, subscription.TrialEndsAtUtc, subscription.CurrentPeriodEndUtc, subscription.IsRestricted(clock.UtcNow));
    }

    private static SubscriptionPlanDto ToDto(SubscriptionPlan plan) =>
        new(plan.Id, plan.Name, plan.Code, plan.MonthlyPricePhp, plan.MaxUsers, plan.MaxDoctors, plan.MaxLocations, plan.MaxPatients, plan.TrialDays, plan.FeaturesJson);
}
