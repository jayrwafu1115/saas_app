using System.Net;
using System.Net.Http.Json;
using Clinic.Domain.Appointments;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class AppointmentEndpointTests
{
    [Fact]
    public async Task Appointment_lifecycle_and_calendar_work()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var fixture = await CreateSchedulingFixtureAsync(client);
        var doctorId = Guid.NewGuid();
        var startsAt = new DateTimeOffset(2026, 6, 3, 9, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt.AddMinutes(30);

        var createResponse = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentRequest(
            fixture.TenantId,
            fixture.LocationId,
            fixture.PatientId,
            doctorId,
            startsAt,
            endsAt,
            "Annual visit",
            "Bring documents"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentResponse>();
        created.Should().NotBeNull();
        created!.Status.Should().Be(AppointmentStatus.Scheduled);

        var conflictResponse = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentRequest(
            fixture.TenantId,
            fixture.LocationId,
            fixture.PatientId,
            doctorId,
            startsAt.AddMinutes(10),
            endsAt.AddMinutes(10),
            "Conflict",
            ""));
        conflictResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var rescheduleResponse = await client.PostAsJsonAsync($"/api/appointments/{created.Id}/reschedule", new RescheduleAppointmentRequest(
            fixture.LocationId,
            doctorId,
            startsAt.AddHours(1),
            endsAt.AddHours(1)));
        rescheduleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var checkInResponse = await client.PostAsync($"/api/appointments/{created.Id}/check-in", null);
        checkInResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var checkedIn = await checkInResponse.Content.ReadFromJsonAsync<AppointmentResponse>();
        checkedIn!.Status.Should().Be(AppointmentStatus.CheckedIn);

        var checkOutResponse = await client.PostAsync($"/api/appointments/{created.Id}/check-out", null);
        checkOutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var checkedOut = await checkOutResponse.Content.ReadFromJsonAsync<AppointmentResponse>();
        checkedOut!.Status.Should().Be(AppointmentStatus.CheckedOut);

        var calendar = await client.GetFromJsonAsync<List<AppointmentResponse>>("/api/appointments/calendar?view=daily&date=2026-06-03");
        calendar.Should().ContainSingle(appointment => appointment.Id == created.Id);
    }

    [Fact]
    public async Task Cancelled_appointment_no_longer_blocks_availability()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var fixture = await CreateSchedulingFixtureAsync(client, "cancel-clinic");
        var doctorId = Guid.NewGuid();
        var startsAt = new DateTimeOffset(2026, 6, 4, 10, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt.AddMinutes(30);

        var createResponse = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentRequest(
            fixture.TenantId,
            fixture.LocationId,
            fixture.PatientId,
            doctorId,
            startsAt,
            endsAt,
            "Consult",
            ""));
        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentResponse>();

        var cancelResponse = await client.PostAsync($"/api/appointments/{created!.Id}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<AppointmentResponse>();
        cancelled!.Status.Should().Be(AppointmentStatus.Cancelled);

        var replacementResponse = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentRequest(
            fixture.TenantId,
            fixture.LocationId,
            fixture.PatientId,
            doctorId,
            startsAt,
            endsAt,
            "Replacement",
            ""));

        replacementResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private static async Task<SchedulingFixture> CreateSchedulingFixtureAsync(HttpClient client, string slug = "appointment-clinic")
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
            "100 Calendar Way",
            "555-0110"));
        locationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = await locationResponse.Content.ReadFromJsonAsync<LocationResponse>();

        var patientResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest(
            tenant.Id,
            location!.Id,
            $"MRN-{slug}",
            "Casey",
            "",
            "Calendar",
            new DateOnly(1991, 2, 3),
            "Other",
            $"{slug}@test.local",
            "555-0202",
            "100 Calendar Way"));
        patientResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        return new SchedulingFixture(tenant.Id, location.Id, patient!.Id);
    }

    private sealed record SchedulingFixture(Guid TenantId, Guid LocationId, Guid PatientId);
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
    private sealed record CreateAppointmentRequest(
        Guid TenantId,
        Guid LocationId,
        Guid PatientId,
        Guid DoctorUserId,
        DateTimeOffset StartsAtUtc,
        DateTimeOffset EndsAtUtc,
        string Reason,
        string? Notes);
    private sealed record RescheduleAppointmentRequest(Guid LocationId, Guid DoctorUserId, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
    private sealed record AppointmentResponse(Guid Id, AppointmentStatus Status);
}
