using System.Net;
using System.Net.Http.Json;
using Clinic.Domain.Clinical;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class EncounterEndpointTests
{
    [Fact]
    public async Task Encounter_lifecycle_timeline_and_exports_work()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var fixture = await CreateClinicalFixtureAsync(client);
        var clinicianId = Guid.NewGuid();
        var encounterDate = new DateTimeOffset(2026, 6, 3, 16, 0, 0, TimeSpan.Zero);

        var createResponse = await client.PostAsJsonAsync("/api/encounters", new CreateEncounterRequest(
            fixture.TenantId,
            fixture.LocationId,
            fixture.PatientId,
            clinicianId,
            null,
            encounterDate,
            "Cough",
            "Patient reports cough for three days.",
            "Lungs clear.",
            "Acute cough.",
            "Hydration and follow-up.",
            "No red flags."));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<EncounterResponse>();
        created.Should().NotBeNull();
        created!.Status.Should().Be(EncounterStatus.Draft);

        var vitalResponse = await client.PostAsJsonAsync($"/api/encounters/{created.Id}/vitals", new AddVitalRequest(
            encounterDate,
            37.1m,
            118,
            76,
            72,
            14,
            98,
            170.2m,
            70.5m,
            "Normal"));
        vitalResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var diagnosisResponse = await client.PostAsJsonAsync($"/api/encounters/{created.Id}/diagnoses", new AddDiagnosisRequest("R05.9", "Cough, unspecified", "Primary"));
        diagnosisResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var prescriptionResponse = await client.PostAsJsonAsync($"/api/encounters/{created.Id}/prescriptions", new AddPrescriptionRequest(
            "Benzonatate",
            "100 mg",
            "Three times daily",
            "5 days",
            "Take as needed for cough."));
        prescriptionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var detail = await client.GetFromJsonAsync<EncounterDetailResponse>($"/api/encounters/{created.Id}");
        detail.Should().NotBeNull();
        detail!.Vitals.Should().ContainSingle();
        detail.Diagnoses.Should().ContainSingle(diagnosis => diagnosis.Code == "R05.9");
        detail.Prescriptions.Should().ContainSingle(prescription => prescription.MedicationName == "Benzonatate");
        detail.AuditLogs.Should().Contain(log => log.Action == "encounter.created");
        detail.AuditLogs.Should().Contain(log => log.Action == "vital.added");

        var timeline = await client.GetFromJsonAsync<List<EncounterTimelineEventResponse>>($"/api/encounters/patients/{fixture.PatientId}/timeline");
        timeline.Should().ContainSingle(item => item.Type == "encounter" && item.Title == "Cough");

        var printResponse = await client.GetAsync($"/api/encounters/{created.Id}/print");
        printResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        printResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/html");

        var pdfResponse = await client.GetAsync($"/api/encounters/{created.Id}/pdf");
        pdfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pdfResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
        pdfBytes[..4].Should().Equal("%PDF"u8.ToArray());

        var signResponse = await client.PostAsync($"/api/encounters/{created.Id}/sign", null);
        signResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var signed = await signResponse.Content.ReadFromJsonAsync<EncounterResponse>();
        signed!.Status.Should().Be(EncounterStatus.Signed);

        var updateSignedResponse = await client.PutAsJsonAsync($"/api/encounters/{created.Id}/soap", new UpdateEncounterSoapRequest(
            fixture.LocationId,
            clinicianId,
            encounterDate,
            "Updated",
            "S",
            "O",
            "A",
            "P",
            null));
        updateSignedResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private static async Task<ClinicalFixture> CreateClinicalFixtureAsync(HttpClient client, string slug = "encounter-clinic")
    {
        var tenantResponse = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest(
            $"Clinic {slug}",
            slug,
            "Active",
            "{}"));
        tenantResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenant = await tenantResponse.Content.ReadFromJsonAsync<TenantResponse>();

        var locationResponse = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest(
            tenant!.Id,
            "Main",
            $"LOC-{slug[..Math.Min(4, slug.Length)].ToUpperInvariant()}",
            "100 Clinical Way",
            "555-0310"));
        locationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = await locationResponse.Content.ReadFromJsonAsync<LocationResponse>();

        var patientResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest(
            tenant.Id,
            location!.Id,
            $"MRN-{slug}",
            "Morgan",
            "",
            "Clinical",
            new DateOnly(1988, 8, 9),
            "Other",
            $"{slug}@test.local",
            "555-0303",
            "100 Clinical Way"));
        patientResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        return new ClinicalFixture(tenant.Id, location.Id, patient!.Id);
    }

    private sealed record ClinicalFixture(Guid TenantId, Guid LocationId, Guid PatientId);
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
    private sealed record UpdateEncounterSoapRequest(
        Guid LocationId,
        Guid ClinicianUserId,
        DateTimeOffset EncounterDateUtc,
        string ChiefComplaint,
        string Subjective,
        string Objective,
        string Assessment,
        string Plan,
        string? Notes);
    private sealed record AddVitalRequest(
        DateTimeOffset RecordedAtUtc,
        decimal? TemperatureCelsius,
        int? SystolicBloodPressure,
        int? DiastolicBloodPressure,
        int? HeartRate,
        int? RespiratoryRate,
        int? OxygenSaturation,
        decimal? HeightCm,
        decimal? WeightKg,
        string? Notes);
    private sealed record AddDiagnosisRequest(string Code, string Description, string Type);
    private sealed record AddPrescriptionRequest(string MedicationName, string Dosage, string Frequency, string Duration, string? Instructions);
    private sealed record EncounterResponse(Guid Id, EncounterStatus Status);
    private sealed record EncounterDetailResponse(
        Guid Id,
        IReadOnlyList<VitalResponse> Vitals,
        IReadOnlyList<DiagnosisResponse> Diagnoses,
        IReadOnlyList<PrescriptionResponse> Prescriptions,
        IReadOnlyList<AuditLogResponse> AuditLogs);
    private sealed record VitalResponse(Guid Id);
    private sealed record DiagnosisResponse(Guid Id, string Code);
    private sealed record PrescriptionResponse(Guid Id, string MedicationName);
    private sealed record AuditLogResponse(Guid Id, string Action);
    private sealed record EncounterTimelineEventResponse(DateTimeOffset OccurredAtUtc, string Type, string Title, string Description);
}
