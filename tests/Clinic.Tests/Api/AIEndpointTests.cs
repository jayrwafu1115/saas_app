using System.Net;
using System.Net.Http.Json;
using Clinic.Domain.AI;
using Clinic.Domain.Clinical;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class AIEndpointTests
{
    [Fact]
    public async Task AI_generation_is_queued_processed_persisted_and_tracked()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var fixture = await CreateEncounterFixtureAsync(client);

        var queueResponse = await client.PostAsJsonAsync("/api/ai/soap-note", new AIGenerationRequest(fixture.EncounterId, "openai", null));
        queueResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var queued = await queueResponse.Content.ReadFromJsonAsync<AIGenerationResponse>();
        queued.Should().NotBeNull();
        queued!.Status.Should().Be(AIGenerationStatus.Queued);

        var completed = await WaitForGenerationAsync(client, queued.Id);
        completed.Status.Should().Be(AIGenerationStatus.Completed);
        completed.Output.Should().Contain("SoapNote");
        completed.TotalTokens.Should().BeGreaterThan(0);
        completed.CostUsd.Should().Be(0);
        completed.LatencyMs.Should().BeGreaterThanOrEqualTo(0);

        var list = await client.GetFromJsonAsync<List<AIGenerationResponse>>($"/api/ai/encounters/{fixture.EncounterId}/generations");
        list.Should().ContainSingle(item => item.Id == queued.Id);

        var usage = await client.GetFromJsonAsync<AIUsageResponse>($"/api/ai/usage?tenantId={fixture.TenantId}");
        usage.Should().NotBeNull();
        usage!.RequestCount.Should().Be(1);
        usage.CompletedCount.Should().Be(1);
        usage.TotalTokens.Should().Be(completed.TotalTokens);

        var cachedResponse = await client.PostAsJsonAsync("/api/ai/soap-note", new AIGenerationRequest(fixture.EncounterId, "openai", null));
        cachedResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var cachedQueued = await cachedResponse.Content.ReadFromJsonAsync<AIGenerationResponse>();
        var cachedCompleted = await WaitForGenerationAsync(client, cachedQueued!.Id);
        cachedCompleted.ServedFromCache.Should().BeTrue();
    }

    private static async Task<AIGenerationResponse> WaitForGenerationAsync(HttpClient client, Guid generationId)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var generation = await client.GetFromJsonAsync<AIGenerationResponse>($"/api/ai/generations/{generationId}");
            if (generation!.Status is AIGenerationStatus.Completed or AIGenerationStatus.Failed)
            {
                return generation;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("AI generation did not complete.");
    }

    private static async Task<EncounterFixture> CreateEncounterFixtureAsync(HttpClient client)
    {
        var tenantResponse = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest("AI Clinic", "ai-clinic", "Active", "{}"));
        tenantResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenant = await tenantResponse.Content.ReadFromJsonAsync<TenantResponse>();

        var locationResponse = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest(tenant!.Id, "Main", "AI-1", "100 AI Way", "555-9000"));
        locationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = await locationResponse.Content.ReadFromJsonAsync<LocationResponse>();

        var patientResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest(
            tenant.Id,
            location!.Id,
            "MRN-AI",
            "Alex",
            "",
            "Prompt",
            new DateOnly(1985, 1, 1),
            "Other",
            "alex.prompt@test.local",
            "555-9001",
            "100 AI Way"));
        patientResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        var encounterResponse = await client.PostAsJsonAsync("/api/encounters", new CreateEncounterRequest(
            tenant.Id,
            location.Id,
            patient!.Id,
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow,
            "Fatigue",
            "Fatigue for one week.",
            "Stable exam.",
            "Likely viral syndrome.",
            "Rest and hydration.",
            ""));
        encounterResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var encounter = await encounterResponse.Content.ReadFromJsonAsync<EncounterResponse>();

        var diagnosisResponse = await client.PostAsJsonAsync($"/api/encounters/{encounter!.Id}/diagnoses", new AddDiagnosisRequest("R53.83", "Other fatigue", "Primary"));
        diagnosisResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        return new EncounterFixture(tenant.Id, encounter.Id);
    }

    private sealed record EncounterFixture(Guid TenantId, Guid EncounterId);
    private sealed record CreateTenantRequest(string Name, string Slug, string Status, string SettingsJson);
    private sealed record TenantResponse(Guid Id);
    private sealed record CreateLocationRequest(Guid TenantId, string Name, string Code, string Address, string Phone);
    private sealed record LocationResponse(Guid Id);
    private sealed record CreatePatientRequest(
        Guid TenantId,
        Guid LocationId,
        string MedicalRecordNumber,
        string FirstName,
        string? MiddleName,
        string LastName,
        DateOnly BirthDate,
        string Gender,
        string Email,
        string Phone,
        string Address);
    private sealed record PatientResponse(Guid Id);
    private sealed record CreateEncounterRequest(
        Guid TenantId,
        Guid LocationId,
        Guid PatientId,
        Guid ClinicianUserId,
        Guid? AppointmentId,
        DateTimeOffset EncounterDateUtc,
        string ChiefComplaint,
        string Subjective,
        string Objective,
        string Assessment,
        string Plan,
        string? Notes);
    private sealed record EncounterResponse(Guid Id, EncounterStatus Status);
    private sealed record AddDiagnosisRequest(string Code, string Description, string Type);
    private sealed record AIGenerationRequest(Guid EncounterId, string Provider, string? Model);
    private sealed record AIGenerationResponse(
        Guid Id,
        AIGenerationStatus Status,
        string Output,
        int TotalTokens,
        decimal CostUsd,
        long LatencyMs,
        bool ServedFromCache);
    private sealed record AIUsageResponse(Guid TenantId, int RequestCount, int CompletedCount, int FailedCount, int TotalTokens, decimal TotalCostUsd, double AverageLatencyMs);
}
