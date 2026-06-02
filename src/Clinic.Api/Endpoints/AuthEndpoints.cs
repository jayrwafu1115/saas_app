using System.Security.Claims;
using Clinic.Application.Common.Interfaces;
using Clinic.Application.Common.Security;
using Clinic.Domain.Users;
using Clinic.Infrastructure.Identity;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clinic.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

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
        IDateTimeProvider dateTimeProvider)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null
            || !user.IsActive
            || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Results.Unauthorized();
        }

        if (!user.EmailConfirmed)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await CreateAuthResponseAsync(
            user,
            userManager,
            dbContext,
            jwtTokenService,
            jwtOptions.Value,
            dateTimeProvider.UtcNow));
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
    private sealed record LoginRequest(string Email, string Password);
    private sealed record RefreshRequest(string AccessToken, string RefreshToken);
    private sealed record ForgotPasswordRequest(string Email);
    private sealed record ForgotPasswordResponse(string? ResetToken);
    private sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
    private sealed record LogoutRequest(string RefreshToken);
    private sealed record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAtUtc, UserProfileResponse User);
    private sealed record UserProfileResponse(
        Guid Id,
        Guid? TenantId,
        string Email,
        string DisplayName,
        IEnumerable<string> Roles,
        IEnumerable<string> Permissions);
}
