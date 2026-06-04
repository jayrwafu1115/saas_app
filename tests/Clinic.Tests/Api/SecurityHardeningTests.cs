using System.Net;
using System.Net.Http.Json;
using Clinic.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Tests.Api;

public sealed class SecurityHardeningTests
{
    [Fact]
    public async Task Health_response_includes_security_headers()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }

    [Fact]
    public async Task Failed_login_writes_security_audit_log()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "missing@test.local",
            "WrongPassword123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditLog = await dbContext.SecurityAuditLogs.AsNoTracking().SingleAsync();
        auditLog.EventType.Should().Be("auth.login_failed");
        auditLog.Subject.Should().Be("missing@test.local");
        auditLog.DetailsJson.Should().Contain("invalid_credentials");
    }

    [Fact]
    public async Task Authenticated_user_can_start_mfa_setup()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();
        await client.AuthenticateAsSuperAdminAsync();

        var response = await client.PostAsync("/api/auth/mfa/setup", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var setup = await response.Content.ReadFromJsonAsync<MfaSetupResponse>();
        setup.Should().NotBeNull();
        setup!.SharedKey.Should().NotBeNullOrWhiteSpace();
        setup.AuthenticatorUri.Should().StartWith("otpauth://totp/");
        setup.IsEnabled.Should().BeFalse();
    }

    private sealed record LoginRequest(string Email, string Password);
    private sealed record MfaSetupResponse(string SharedKey, string AuthenticatorUri, bool IsEnabled);
}
