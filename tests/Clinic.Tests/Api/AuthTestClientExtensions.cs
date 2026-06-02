using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Clinic.Tests.Api;

internal static class AuthTestClientExtensions
{
    public static async Task AuthenticateAsSuperAdminAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "superadmin@test.local",
            "SuperAdmin123!"));
        var content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull(content);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }

    private sealed record LoginRequest(string Email, string Password);
    private sealed record AuthResponse(string AccessToken, string RefreshToken);
}
