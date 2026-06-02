using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class TenantLocationEndpointTests
{
    [Fact]
    public async Task Tenant_endpoints_create_and_list_tenants()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();

        var createResponse = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest(
            "North Clinic",
            "north-clinic",
            "Active",
            "{}"));

        var createContent = await createResponse.Content.ReadAsStringAsync();
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, createContent);
        var created = await createResponse.Content.ReadFromJsonAsync<TenantResponse>();
        created.Should().NotBeNull();
        created!.Name.Should().Be("North Clinic");
        created.Slug.Should().Be("north-clinic");
        created.Status.Should().Be("Active");
        created.SettingsJson.Should().Be("{}");

        var list = await client.GetFromJsonAsync<List<TenantResponse>>("/api/tenants");

        list.Should().ContainSingle(tenant => tenant.Id == created.Id);
    }

    [Fact]
    public async Task Location_endpoints_create_and_filter_locations_by_tenant_header()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();

        var firstTenant = await CreateTenantAsync(client, "West Clinic", "west-clinic");
        var secondTenant = await CreateTenantAsync(client, "East Clinic", "east-clinic");

        var firstLocationResponse = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest(
            firstTenant.Id,
            "Front Desk",
            "fd",
            "100 Main Street",
            "555-0100"));
        firstLocationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondLocationResponse = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest(
            secondTenant.Id,
            "Intake",
            "in",
            "200 Main Street",
            "555-0101"));
        secondLocationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/locations");
        request.Headers.Add("X-Tenant-Id", firstTenant.Id.ToString());

        var filteredResponse = await client.SendAsync(request);

        filteredResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var locations = await filteredResponse.Content.ReadFromJsonAsync<List<LocationResponse>>();
        locations.Should().ContainSingle();
        locations![0].TenantId.Should().Be(firstTenant.Id);
        locations[0].Code.Should().Be("FD");
    }

    private static async Task<TenantResponse> CreateTenantAsync(HttpClient client, string name, string slug)
    {
        var response = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest(name, slug, "Active", "{}"));
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, content);
        return (await response.Content.ReadFromJsonAsync<TenantResponse>())!;
    }

    private sealed record CreateTenantRequest(string Name, string Slug, string Status, string SettingsJson);
    private sealed record TenantResponse(Guid Id, string Name, string Slug, string Status, string SettingsJson);
    private sealed record CreateLocationRequest(Guid TenantId, string Name, string Code, string Address, string Phone);
    private sealed record LocationResponse(Guid Id, Guid TenantId, string Name, string Code, string Address, string Phone);
}
