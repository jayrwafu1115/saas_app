using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class PatientEndpointTests
{
    [Fact]
    public async Task Patient_crud_search_soft_delete_and_timeline_work()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var tenant = await CreateTenantAsync(client);
        var location = await CreateLocationAsync(client, tenant.Id);

        var createResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest(
            tenant.Id,
            location.Id,
            "MRN-001",
            "Ava",
            "R",
            "Stone",
            new DateOnly(1990, 5, 1),
            "Female",
            "ava.stone@test.local",
            "555-1212",
            "100 Health Way"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<PatientResponse>();
        created.Should().NotBeNull();
        created!.MedicalRecordNumber.Should().Be("MRN-001");

        var search = await client.GetFromJsonAsync<PagedPatientResponse>("/api/patients?search=ava&pageNumber=1&pageSize=10");
        search!.TotalCount.Should().Be(1);
        search.Items.Should().ContainSingle(patient => patient.Id == created.Id);

        var updateResponse = await client.PutAsJsonAsync($"/api/patients/{created.Id}", new UpdatePatientRequest(
            location.Id,
            "MRN-001",
            "Ava",
            "R",
            "Stone",
            new DateOnly(1990, 5, 1),
            "Female",
            "ava.updated@test.local",
            "555-3434",
            "200 Health Way"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var contactResponse = await client.PostAsJsonAsync($"/api/patients/{created.Id}/contacts", new CreateContactRequest(
            "Morgan Stone",
            "Spouse",
            "morgan@test.local",
            "555-9999",
            true));
        contactResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent("patient document"u8.ToArray());
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(file, "file", "intake.txt");
        var documentResponse = await client.PostAsync($"/api/patients/{created.Id}/documents", form);
        documentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var timeline = await client.GetFromJsonAsync<List<TimelineResponse>>($"/api/patients/{created.Id}/timeline");
        timeline.Should().NotBeNull();
        timeline!.Should().Contain(item => item.Type == "patient.created");
        timeline.Should().Contain(item => item.Type == "document.uploaded");
        timeline.Should().Contain(item => item.Type == "contact.added");

        var deleteResponse = await client.DeleteAsync($"/api/patients/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var emptySearch = await client.GetFromJsonAsync<PagedPatientResponse>("/api/patients?search=ava&pageNumber=1&pageSize=10");
        emptySearch!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Patient_search_supports_pagination()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();
        var tenant = await CreateTenantAsync(client, "Page Clinic", "page-clinic");
        var location = await CreateLocationAsync(client, tenant.Id);

        for (var i = 1; i <= 3; i++)
        {
            var response = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest(
                tenant.Id,
                location.Id,
                $"PAGE-{i:000}",
                $"Patient{i}",
                "",
                "Paged",
                new DateOnly(1985, 1, i),
                "Other",
                $"patient{i}@test.local",
                $"555-00{i}",
                "100 Page Way"));
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var page = await client.GetFromJsonAsync<PagedPatientResponse>("/api/patients?pageNumber=2&pageSize=2");

        page.Should().NotBeNull();
        page!.PageNumber.Should().Be(2);
        page.PageSize.Should().Be(2);
        page.TotalCount.Should().Be(3);
        page.Items.Should().ContainSingle();
    }

    private static async Task<TenantResponse> CreateTenantAsync(HttpClient client, string name = "Patient Clinic", string slug = "patient-clinic")
    {
        var response = await client.PostAsJsonAsync("/api/tenants", new CreateTenantRequest(name, slug, "Active", "{}"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TenantResponse>())!;
    }

    private static async Task<LocationResponse> CreateLocationAsync(HttpClient client, Guid tenantId)
    {
        var response = await client.PostAsJsonAsync("/api/locations", new CreateLocationRequest(
            tenantId,
            "Main",
            $"MAIN-{tenantId.ToString()[..4]}",
            "100 Health Way",
            "555-0100"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<LocationResponse>())!;
    }

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
    private sealed record UpdatePatientRequest(
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
    private sealed record CreateContactRequest(string Name, string Relationship, string Email, string Phone, bool IsPrimary);
    private sealed record PatientResponse(Guid Id, string MedicalRecordNumber);
    private sealed record PagedPatientResponse(List<PatientResponse> Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);
    private sealed record TimelineResponse(string Type);
}
