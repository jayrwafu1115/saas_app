using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class FoundationEndpointTests
{
    [Fact]
    public async Task Foundation_endpoint_returns_phase_one_status()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/foundation");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<FoundationStatus>();
        payload.Should().NotBeNull();
        payload!.Phase.Should().Be("Phase 1");
        payload.Status.Should().Be("Foundation ready");
    }

    private sealed record FoundationStatus(string Service, string Phase, string Status);
}
