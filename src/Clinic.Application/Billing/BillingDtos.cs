using Clinic.Domain.Billing;

namespace Clinic.Application.Billing;

public sealed record SubscriptionPlanDto(Guid Id, string Name, string Code, decimal MonthlyPricePhp, int MaxUsers, int MaxDoctors, int MaxLocations, int MaxPatients, int TrialDays, string FeaturesJson);
public sealed record TenantSubscriptionDto(Guid Id, Guid TenantId, Guid PlanId, string PlanName, SubscriptionStatus Status, DateTimeOffset TrialEndsAtUtc, DateTimeOffset CurrentPeriodEndUtc, bool IsRestricted);
public sealed record SubscriptionUsageDto(Guid TenantId, string Metric, int Quantity, int Limit, DateOnly Period, bool IsOverLimit);
public sealed record BillingCheckoutDto(Guid PaymentId, BillingProvider Provider, decimal AmountPhp, string CheckoutUrl, string ProviderReference);
public sealed record SubscriptionOverviewDto(IReadOnlyList<TenantSubscriptionDto> Subscriptions, IReadOnlyList<SubscriptionUsageDto> Usage);
public sealed record TenantRestrictionDto(Guid TenantId, bool IsRestricted, string Reason);
public sealed record BillingProviderCheckout(string ProviderReference, string CheckoutUrl);
