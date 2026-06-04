using System.Net;
using System.Net.Http.Json;
using Clinic.Domain.Billing;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class BillingEndpointTests
{
    [Fact]
    public async Task Billing_plans_trial_usage_restrictions_and_checkout_work()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var tenant = await CreateTenantAsync(client);
        var location = await CreateLocationAsync(client, tenant.Id);
        await CreatePatientAsync(client, tenant.Id, location.Id);

        var plans = await client.GetFromJsonAsync<List<PlanResponse>>("/api/billing/plans");
        plans.Should().Contain(plan => plan.Code == "starter");
        plans.Should().Contain(plan => plan.Code == "professional");
        plans.Should().Contain(plan => plan.Code == "enterprise");

        var trialResponse = await client.PostAsJsonAsync("/api/billing/trial", new TrialRequest(tenant.Id, "starter"));
        trialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var trial = await trialResponse.Content.ReadFromJsonAsync<SubscriptionResponse>();
        trial!.Status.Should().Be(SubscriptionStatus.Trialing);
        trial.PlanName.Should().Be("Starter");
        trial.IsRestricted.Should().BeFalse();

        var overview = await client.GetFromJsonAsync<OverviewResponse>($"/api/billing/overview?tenantId={tenant.Id}");
        overview!.Subscriptions.Should().ContainSingle(subscription => subscription.TenantId == tenant.Id);
        overview.Usage.Should().Contain(usage => usage.Metric == "locations" && usage.Quantity == 1 && usage.Limit == 1);
        overview.Usage.Should().Contain(usage => usage.Metric == "patients" && usage.Quantity == 1 && usage.Limit == 500);

        var restriction = await client.GetFromJsonAsync<RestrictionResponse>($"/api/billing/tenants/{tenant.Id}/restriction");
        restriction!.IsRestricted.Should().BeFalse();

        var gcashResponse = await client.PostAsJsonAsync("/api/billing/checkout", new CheckoutRequest(tenant.Id, "professional", BillingProvider.GCash));
        gcashResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var gcash = await gcashResponse.Content.ReadFromJsonAsync<CheckoutResponse>();
        gcash!.Provider.Should().Be(BillingProvider.GCash);
        gcash.CheckoutUrl.Should().Contain("/gcash");
        gcash.AmountPhp.Should().Be(4999);

        var mayaResponse = await client.PostAsJsonAsync("/api/billing/checkout", new CheckoutRequest(tenant.Id, "enterprise", BillingProvider.Maya));
        mayaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var maya = await mayaResponse.Content.ReadFromJsonAsync<CheckoutResponse>();
        maya!.Provider.Should().Be(BillingProvider.Maya);
        maya.CheckoutUrl.Should().Contain("/maya");
        maya.AmountPhp.Should().Be(14999);
    }

    private static async Task<TenantResponse> CreateTenantAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest("Billing Clinic", "billing-clinic", "Active", "{}"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TenantResponse>())!;
    }

    private static async Task<LocationResponse> CreateLocationAsync(HttpClient client, Guid tenantId)
    {
        var response = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest(tenantId, "Main", "BIL", "100 Billing Way", "555-8000"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<LocationResponse>())!;
    }

    private static async Task CreatePatientAsync(HttpClient client, Guid tenantId, Guid locationId)
    {
        var response = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest(
            tenantId,
            locationId,
            "MRN-BILLING",
            "Billie",
            "",
            "Sub",
            new DateOnly(1994, 1, 1),
            "Other",
            "billie.sub@test.local",
            "555-8001",
            "100 Billing Way"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private sealed record CreateTenantRequest(string Name, string Slug, string Status, string SettingsJson);
    private sealed record TenantResponse(Guid Id);
    private sealed record CreateLocationRequest(Guid TenantId, string Name, string Code, string Address, string Phone);
    private sealed record LocationResponse(Guid Id);
    private sealed record CreatePatientRequest(Guid TenantId, Guid LocationId, string MedicalRecordNumber, string FirstName, string? MiddleName, string LastName, DateOnly BirthDate, string Gender, string Email, string Phone, string Address);
    private sealed record PlanResponse(Guid Id, string Name, string Code, decimal MonthlyPricePhp);
    private sealed record TrialRequest(Guid TenantId, string PlanCode);
    private sealed record CheckoutRequest(Guid TenantId, string PlanCode, BillingProvider Provider);
    private sealed record SubscriptionResponse(Guid Id, Guid TenantId, string PlanName, SubscriptionStatus Status, bool IsRestricted);
    private sealed record CheckoutResponse(Guid PaymentId, BillingProvider Provider, decimal AmountPhp, string CheckoutUrl, string ProviderReference);
    private sealed record OverviewResponse(IReadOnlyList<SubscriptionResponse> Subscriptions, IReadOnlyList<UsageResponse> Usage);
    private sealed record UsageResponse(Guid TenantId, string Metric, int Quantity, int Limit, bool IsOverLimit);
    private sealed record RestrictionResponse(Guid TenantId, bool IsRestricted, string Reason);
}
