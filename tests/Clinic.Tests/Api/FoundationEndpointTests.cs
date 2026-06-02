using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Clinic.Tests.Api;

public sealed class FoundationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FoundationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Foundation_endpoint_returns_phase_one_status()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/foundation");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<FoundationStatus>();
        payload.Should().NotBeNull();
        payload!.Phase.Should().Be("Phase 1");
        payload.Status.Should().Be("Foundation ready");
    }

    private sealed record FoundationStatus(string Service, string Phase, string Status);
}
