using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class ReportingEndpointTests
{
    [Fact]
    public async Task Reporting_dashboard_charts_and_exports_work()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var fixture = await CreateReportingFixtureAsync(client);
        var doctorId = Guid.NewGuid();

        var appointmentResponse = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentRequest(
            fixture.TenantId,
            fixture.LocationId,
            fixture.PatientId,
            doctorId,
            new DateTimeOffset(2026, 6, 4, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 4, 9, 30, 0, TimeSpan.Zero),
            "Report visit",
            ""));
        appointmentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var appointment = await appointmentResponse.Content.ReadFromJsonAsync<AppointmentResponse>();

        (await client.PostAsync($"/api/appointments/{appointment!.Id}/check-in", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsync($"/api/appointments/{appointment.Id}/check-out", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboard = await client.GetFromJsonAsync<ReportingDashboardResponse>($"/api/reports/dashboard?tenantId={fixture.TenantId}&from=2026-06-01&to=2026-06-30");
        dashboard.Should().NotBeNull();
        dashboard!.Kpis.TotalPatients.Should().Be(1);
        dashboard.Kpis.Appointments.Should().Be(1);
        dashboard.Kpis.Revenue.Should().Be(125);
        dashboard.Kpis.ActiveDoctors.Should().Be(1);
        dashboard.Charts.DailyVisits.Should().ContainSingle(item => item.Visits == 1);
        dashboard.Charts.MonthlyRevenue.Should().ContainSingle(item => item.Revenue == 125);
        dashboard.Charts.DoctorPerformance.Should().ContainSingle(item => item.DoctorUserId == doctorId && item.CompletedVisits == 1);
        dashboard.Charts.LocationPerformance.Should().ContainSingle(item => item.LocationId == fixture.LocationId && item.CompletedVisits == 1);

        var excel = await client.GetAsync($"/api/reports/export/excel?tenantId={fixture.TenantId}&from=2026-06-01&to=2026-06-30");
        excel.StatusCode.Should().Be(HttpStatusCode.OK);
        excel.Content.Headers.ContentType!.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var excelBytes = await excel.Content.ReadAsByteArrayAsync();
        excelBytes[..2].Should().Equal("PK"u8.ToArray());

        var pdf = await client.GetAsync($"/api/reports/export/pdf?tenantId={fixture.TenantId}&from=2026-06-01&to=2026-06-30");
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var pdfBytes = await pdf.Content.ReadAsByteArrayAsync();
        pdfBytes[..4].Should().Equal("%PDF"u8.ToArray());
    }

    private static async Task<ReportingFixture> CreateReportingFixtureAsync(HttpClient client)
    {
        var tenantResponse = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest("Report Clinic", "report-clinic", "Active", "{}"));
        tenantResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenant = await tenantResponse.Content.ReadFromJsonAsync<TenantResponse>();

        var locationResponse = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest(tenant!.Id, "Main", "REP", "100 Report Way", "555-7000"));
        locationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = await locationResponse.Content.ReadFromJsonAsync<LocationResponse>();

        var patientResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest(
            tenant.Id,
            location!.Id,
            "MRN-REPORT",
            "Riley",
            "",
            "Report",
            new DateOnly(1990, 4, 1),
            "Other",
            "riley.report@test.local",
            "555-7001",
            "100 Report Way"));
        patientResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var patient = await patientResponse.Content.ReadFromJsonAsync<PatientResponse>();

        return new ReportingFixture(tenant.Id, location.Id, patient!.Id);
    }

    private sealed record ReportingFixture(Guid TenantId, Guid LocationId, Guid PatientId);
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
    private sealed record CreateAppointmentRequest(Guid TenantId, Guid LocationId, Guid PatientId, Guid DoctorUserId, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, string Reason, string? Notes);
    private sealed record AppointmentResponse(Guid Id);
    private sealed record ReportingDashboardResponse(DashboardKpisResponse Kpis, ReportingChartsResponse Charts);
    private sealed record DashboardKpisResponse(int TotalPatients, int NewPatients, int Appointments, decimal Revenue, int ActiveDoctors);
    private sealed record ReportingChartsResponse(
        IReadOnlyList<DailyVisitResponse> DailyVisits,
        IReadOnlyList<MonthlyRevenueResponse> MonthlyRevenue,
        IReadOnlyList<DoctorPerformanceResponse> DoctorPerformance,
        IReadOnlyList<LocationPerformanceResponse> LocationPerformance);
    private sealed record DailyVisitResponse(DateOnly Date, int Visits);
    private sealed record MonthlyRevenueResponse(int Year, int Month, decimal Revenue);
    private sealed record DoctorPerformanceResponse(Guid DoctorUserId, int Appointments, int CompletedVisits, decimal Revenue);
    private sealed record LocationPerformanceResponse(Guid LocationId, string LocationName, int Appointments, int CompletedVisits, decimal Revenue);
}
