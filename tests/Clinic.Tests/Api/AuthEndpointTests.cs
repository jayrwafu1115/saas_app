using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Clinic.Tests.Api;

public sealed class AuthEndpointTests
{
    [Fact]
    public async Task Register_confirm_login_and_me_complete_authentication_flow()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "owner@test.local",
            "OwnerPassword123!",
            "Clinic Owner",
            null,
            null));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registration = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        registration.Should().NotBeNull();

        var unconfirmedLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "owner@test.local",
            "OwnerPassword123!"));
        unconfirmedLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var confirmResponse = await client.PostAsJsonAsync("/api/auth/confirm-email", new ConfirmEmailRequest(
            registration!.UserId,
            registration.EmailVerificationToken));
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "owner@test.local",
            "OwnerPassword123!"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        auth.User.Roles.Should().Contain("Clinic Owner");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var me = await client.GetFromJsonAsync<UserProfileResponse>("/api/auth/me");

        me.Should().NotBeNull();
        me!.Email.Should().Be("owner@test.local");
    }

    [Fact]
    public async Task Refresh_and_password_reset_issue_new_credentials()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "reset@test.local",
            "ResetPassword123!",
            "Reset User",
            null,
            null));
        var registration = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        await client.PostAsJsonAsync("/api/auth/confirm-email", new ConfirmEmailRequest(
            registration!.UserId,
            registration.EmailVerificationToken));

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "reset@test.local",
            "ResetPassword123!"));
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(
            auth!.AccessToken,
            auth.RefreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        refreshed!.RefreshToken.Should().NotBe(auth.RefreshToken);

        var forgotResponse = await client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest("reset@test.local"));
        forgotResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var forgot = await forgotResponse.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        forgot!.ResetToken.Should().NotBeNullOrWhiteSpace();

        var resetResponse = await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest(
            "reset@test.local",
            forgot.ResetToken!,
            "ResetPassword456!"));
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var oldLoginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "reset@test.local",
            "ResetPassword123!"));
        oldLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var newLoginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "reset@test.local",
            "ResetPassword456!"));
        newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Roles_endpoint_returns_seeded_roles_and_permissions()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();

        var rolesResponse = await client.GetAsync("/api/roles");
        var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.OK, rolesContent);
        var roles = await rolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();

        roles.Should().NotBeNull();
        roles!.Should().HaveCount(7);
        roles.Should().Contain(role => role.Name == "Patient");
        roles.Single(role => role.Name == "Super Admin").Permissions.Should().Contain("roles.manage");
    }

    private sealed record RegisterRequest(string Email, string Password, string DisplayName, Guid? TenantId, string? Role);
    private sealed record RegisterResponse(Guid UserId, string Email, string EmailVerificationToken);
    private sealed record ConfirmEmailRequest(Guid UserId, string Token);
    private sealed record LoginRequest(string Email, string Password);
    private sealed record RefreshRequest(string AccessToken, string RefreshToken);
    private sealed record ForgotPasswordRequest(string Email);
    private sealed record ForgotPasswordResponse(string? ResetToken);
    private sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
    private sealed record AuthResponse(string AccessToken, string RefreshToken, UserProfileResponse User);
    private sealed record UserProfileResponse(string Email, string[] Roles);
    private sealed record RoleResponse(string Name, string[] Permissions);
}
