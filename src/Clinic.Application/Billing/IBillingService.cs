using Clinic.Domain.Billing;

namespace Clinic.Application.Billing;

public interface IBillingService
{
    Task<IReadOnlyList<SubscriptionPlanDto>> ListPlansAsync(CancellationToken cancellationToken);
    Task<TenantSubscriptionDto> StartTrialAsync(Guid tenantId, string planCode, CancellationToken cancellationToken);
    Task<BillingCheckoutDto> CreateCheckoutAsync(Guid tenantId, string planCode, BillingProvider provider, CancellationToken cancellationToken);
    Task<SubscriptionOverviewDto> GetOverviewAsync(Guid? tenantId, CancellationToken cancellationToken);
    Task<TenantRestrictionDto> GetRestrictionAsync(Guid tenantId, CancellationToken cancellationToken);
}
