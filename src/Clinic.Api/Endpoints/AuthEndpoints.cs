using System.Security.Claims;
using Clinic.Application.Common.Interfaces;
using Clinic.Application.Common.Security;
using Clinic.Domain.Users;
using Clinic.Infrastructure.Identity;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clinic.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .RequireRateLimiting("auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/confirm-email", ConfirmEmailAsync)
            .WithName("ConfirmEmail")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshAsync)
            .WithName("RefreshToken")
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .Produces<ForgotPasswordResponse>();

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/mfa/setup", SetupMfaAsync)
            .RequireAuthorization()
            .WithName("SetupMfa")
            .Produces<MfaSetupResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/mfa/enable", EnableMfaAsync)
            .RequireAuthorization()
            .WithName("EnableMfa")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/mfa/disable", DisableMfaAsync)
            .RequireAuthorization()
            .WithName("DisableMfa")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/me", MeAsync)
            .RequireAuthorization()
            .WithName("Me")
            .Produces<UserProfileResponse>();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ISecurityAuditService securityAuditService,
        HttpContext httpContext,
        IDateTimeProvider dateTimeProvider)
    {
        if (!string.IsNullOrWhiteSpace(request.Role) && request.Role != ApplicationRoleNames.ClinicOwner)
        {
            return Results.BadRequest(new { message = "Role assignment is managed after registration." });
        }

        var role = ApplicationRoleNames.ClinicOwner;

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            DisplayName = request.DisplayName.Trim(),
            TenantId = request.TenantId
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ToValidationProblem(result);
        }

        await userManager.AddToRoleAsync(user, role);
        var emailVerificationToken = await CreateUserActionTokenAsync(
            user.Id,
            UserActionTokenPurposes.EmailVerification,
            TimeSpan.FromDays(2),
            dateTimeProvider,
            dbContext);

        await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
            httpContext,
            user.TenantId,
            user.Id,
            "auth.registered",
            user.Email ?? request.Email,
            """{"role":"Clinic Owner"}"""));

        return Results.Created($"/api/auth/users/{user.Id}", new RegisterResponse(
            user.Id,
            user.Email!,
            emailVerificationToken));
    }

    private static async Task<IResult> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Results.BadRequest(new { message = "Invalid confirmation request." });
        }

        var token = await FindActiveUserActionTokenAsync(
            dbContext,
            user.Id,
            UserActionTokenPurposes.EmailVerification,
            request.Token,
            dateTimeProvider.UtcNow);
        if (token is null)
        {
            return Results.BadRequest(new { message = "Invalid confirmation request." });
        }

        token.ConsumedAtUtc = dateTimeProvider.UtcNow;
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
        await dbContext.SaveChangesAsync();
        return Results.Ok(new { message = "Email confirmed." });
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions,
        ISecurityAuditService securityAuditService,
        HttpContext httpContext,
        IDateTimeProvider dateTimeProvider)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null
            || !user.IsActive
            || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
                httpContext,
                user?.TenantId,
                user?.Id,
                "auth.login_failed",
                request.Email.Trim(),
                """{"reason":"invalid_credentials"}"""));
            return Results.Unauthorized();
        }

        if (!user.EmailConfirmed)
        {
            await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
                httpContext,
                user.TenantId,
                user.Id,
                "auth.login_failed",
                user.Email ?? request.Email,
                """{"reason":"email_not_confirmed"}"""));
            return Results.Unauthorized();
        }

        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.MfaCode)
                || !await userManager.VerifyTwoFactorTokenAsync(
                    user,
                    TokenOptions.DefaultAuthenticatorProvider,
                    NormalizeMfaCode(request.MfaCode)))
            {
                await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
                    httpContext,
                    user.TenantId,
                    user.Id,
                    "auth.mfa_failed",
                    user.Email ?? request.Email,
                    """{"reason":"invalid_or_missing_code"}"""));
                return Results.Unauthorized();
            }
        }

        var response = await CreateAuthResponseAsync(
            user,
            userManager,
            dbContext,
            jwtTokenService,
            jwtOptions.Value,
            dateTimeProvider.UtcNow);

        await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
            httpContext,
            user.TenantId,
            user.Id,
            "auth.login_succeeded",
            user.Email ?? request.Email,
            user.TwoFactorEnabled ? """{"mfa":true}""" : """{"mfa":false}"""));

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshRequest request,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions,
        IDateTimeProvider dateTimeProvider)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch
        {
            return Results.Unauthorized();
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
        {
            return Results.Unauthorized();
        }

        var tokenHash = RefreshTokenGenerator.Hash(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.UserId == user.Id && token.TokenHash == tokenHash);
        if (refreshToken is null || !refreshToken.IsActive(dateTimeProvider.UtcNow))
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await CreateAuthResponseAsync(
            user,
            userManager,
            dbContext,
            jwtTokenService,
            jwtOptions.Value,
            dateTimeProvider.UtcNow,
            refreshToken));
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return Results.Ok(new ForgotPasswordResponse(null));
        }

        var token = await CreateUserActionTokenAsync(
            user.Id,
            UserActionTokenPurposes.PasswordReset,
            TimeSpan.FromHours(2),
            dateTimeProvider,
            dbContext);
        return Results.Ok(new ForgotPasswordResponse(token));
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return Results.BadRequest(new { message = "Invalid reset request." });
        }

        var token = await FindActiveUserActionTokenAsync(
            dbContext,
            user.Id,
            UserActionTokenPurposes.PasswordReset,
            request.Token,
            dateTimeProvider.UtcNow);
        if (token is null)
        {
            return Results.BadRequest(new { message = "Invalid reset request." });
        }

        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, request.NewPassword);
        token.ConsumedAtUtc = dateTimeProvider.UtcNow;
        var result = await userManager.UpdateAsync(user);
        await dbContext.SaveChangesAsync();
        return result.Succeeded ? Results.Ok(new { message = "Password reset." }) : ToValidationProblem(result);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        ClaimsPrincipal principal,
        ApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdValue is null || !Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Ok(new { message = "Logged out." });
        }

        var tokenHash = RefreshTokenGenerator.Hash(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.UserId == userId && token.TokenHash == tokenHash);
        if (refreshToken is not null && refreshToken.RevokedAtUtc is null)
        {
            refreshToken.RevokedAtUtc = dateTimeProvider.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        return Results.Ok(new { message = "Logged out." });
    }

    private static async Task<IResult> SetupMfaAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ISecurityAuditService securityAuditService,
        HttpContext httpContext)
    {
        var user = await GetCurrentUserAsync(principal, userManager);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
            httpContext,
            user.TenantId,
            user.Id,
            "auth.mfa_setup_started",
            user.Email ?? user.Id.ToString()));

        return Results.Ok(new MfaSetupResponse(
            key ?? string.Empty,
            BuildAuthenticatorUri(user.Email ?? user.UserName ?? user.Id.ToString(), key ?? string.Empty),
            user.TwoFactorEnabled));
    }

    private static async Task<IResult> EnableMfaAsync(
        MfaCodeRequest request,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ISecurityAuditService securityAuditService,
        HttpContext httpContext)
    {
        var user = await GetCurrentUserAsync(principal, userManager);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            NormalizeMfaCode(request.Code));
        if (!isValid)
        {
            await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
                httpContext,
                user.TenantId,
                user.Id,
                "auth.mfa_enable_failed",
                user.Email ?? user.Id.ToString()));
            return Results.BadRequest(new { message = "Invalid MFA code." });
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
            httpContext,
            user.TenantId,
            user.Id,
            "auth.mfa_enabled",
            user.Email ?? user.Id.ToString()));

        return Results.Ok(new MfaEnabledResponse(recoveryCodes?.ToArray() ?? []));
    }

    private static async Task<IResult> DisableMfaAsync(
        MfaCodeRequest request,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ISecurityAuditService securityAuditService,
        HttpContext httpContext)
    {
        var user = await GetCurrentUserAsync(principal, userManager);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (user.TwoFactorEnabled)
        {
            var isValid = await userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultAuthenticatorProvider,
                NormalizeMfaCode(request.Code));
            if (!isValid)
            {
                return Results.BadRequest(new { message = "Invalid MFA code." });
            }
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        await securityAuditService.RecordAsync(CreateSecurityAuditEntry(
            httpContext,
            user.TenantId,
            user.Id,
            "auth.mfa_disabled",
            user.Email ?? user.Id.ToString()));

        return Results.Ok(new { message = "MFA disabled." });
    }

    private static async Task<IResult> MeAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(dbContext, roles);
        return Results.Ok(new UserProfileResponse(
            user.Id,
            user.TenantId,
            user.Email ?? string.Empty,
            user.DisplayName,
            roles.ToArray(),
            permissions));
    }

    private static async Task<ApplicationUser?> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null ? null : await userManager.FindByIdAsync(userId);
    }

    private static SecurityAuditEntry CreateSecurityAuditEntry(
        HttpContext httpContext,
        Guid? tenantId,
        Guid? userId,
        string eventType,
        string subject,
        string detailsJson = "{}") =>
        new(
            tenantId,
            userId,
            eventType,
            subject,
            httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            httpContext.Request.Headers.UserAgent.ToString(),
            detailsJson);

    private static string NormalizeMfaCode(string code) =>
        code.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

    private static string BuildAuthenticatorUri(string email, string key) =>
        $"otpauth://totp/{Uri.EscapeDataString("Clinic Management SaaS")}:{Uri.EscapeDataString(email)}?secret={Uri.EscapeDataString(key)}&issuer={Uri.EscapeDataString("Clinic Management SaaS")}&digits=6";

    private static async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        JwtOptions jwtOptions,
        DateTimeOffset utcNow,
        RefreshToken? tokenToRevoke = null)
    {
        var roles = await userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(dbContext, roles);
        var accessToken = jwtTokenService.CreateAccessToken(new JwtUser(
            user.Id,
            user.TenantId,
            user.Email ?? string.Empty,
            user.DisplayName,
            roles.ToArray(),
            permissions));
        var refreshToken = RefreshTokenGenerator.CreateToken();
        var refreshTokenHash = RefreshTokenGenerator.Hash(refreshToken);

        if (tokenToRevoke is not null)
        {
            tokenToRevoke.RevokedAtUtc = utcNow;
            tokenToRevoke.ReplacedByTokenHash = refreshTokenHash;
        }

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = utcNow,
            ExpiresAtUtc = utcNow.AddDays(jwtOptions.RefreshTokenExpirationDays)
        });
        await dbContext.SaveChangesAsync();

        return new AuthResponse(
            accessToken,
            refreshToken,
            utcNow.AddMinutes(jwtOptions.ExpirationMinutes),
            new UserProfileResponse(
                user.Id,
                user.TenantId,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles,
                permissions));
    }

    private static async Task<string> CreateUserActionTokenAsync(
        Guid userId,
        string purpose,
        TimeSpan lifetime,
        IDateTimeProvider dateTimeProvider,
        ApplicationDbContext dbContext)
    {
        var token = RefreshTokenGenerator.CreateToken();
        dbContext.UserActionTokens.Add(new UserActionToken
        {
            UserId = userId,
            Purpose = purpose,
            TokenHash = RefreshTokenGenerator.Hash(token),
            CreatedAtUtc = dateTimeProvider.UtcNow,
            ExpiresAtUtc = dateTimeProvider.UtcNow.Add(lifetime)
        });
        await dbContext.SaveChangesAsync();
        return token;
    }

    private static Task<UserActionToken?> FindActiveUserActionTokenAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        string purpose,
        string token,
        DateTimeOffset utcNow)
    {
        var tokenHash = RefreshTokenGenerator.Hash(token);
        return dbContext.UserActionTokens.FirstOrDefaultAsync(userActionToken =>
            userActionToken.UserId == userId
            && userActionToken.Purpose == purpose
            && userActionToken.TokenHash == tokenHash
            && userActionToken.ConsumedAtUtc == null
            && userActionToken.ExpiresAtUtc > utcNow);
    }

    private static async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        ApplicationDbContext dbContext,
        IEnumerable<string> roles)
    {
        var roleNames = roles.ToArray();
        return await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission => roleNames.Contains(rolePermission.Role.Name!))
            .Select(rolePermission => rolePermission.Permission.Name)
            .Distinct()
            .OrderBy(permission => permission)
            .ToListAsync();
    }

    private static IResult ToValidationProblem(IdentityResult result) =>
        Results.ValidationProblem(result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));

    private sealed record RegisterRequest(string Email, string Password, string DisplayName, Guid? TenantId, string? Role);
    private sealed record RegisterResponse(Guid UserId, string Email, string EmailVerificationToken);
    private sealed record ConfirmEmailRequest(Guid UserId, string Token);
    private sealed record LoginRequest(string Email, string Password, string? MfaCode = null);
    private sealed record RefreshRequest(string AccessToken, string RefreshToken);
    private sealed record ForgotPasswordRequest(string Email);
    private sealed record ForgotPasswordResponse(string? ResetToken);
    private sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
    private sealed record LogoutRequest(string RefreshToken);
    private sealed record MfaCodeRequest(string Code);
    private sealed record MfaSetupResponse(string SharedKey, string AuthenticatorUri, bool IsEnabled);
    private sealed record MfaEnabledResponse(string[] RecoveryCodes);
    private sealed record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAtUtc, UserProfileResponse User);
    private sealed record UserProfileResponse(
        Guid Id,
        Guid? TenantId,
        string Email,
        string DisplayName,
        IEnumerable<string> Roles,
        IEnumerable<string> Permissions);
}
