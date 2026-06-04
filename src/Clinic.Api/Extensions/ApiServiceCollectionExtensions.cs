using System.Threading.RateLimiting;
using System.Text;
using Clinic.Api.Options;
using Clinic.Application.Common.Interfaces;
using Clinic.Application.Common.Security;
using Clinic.Domain.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Clinic.Api.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<TenantContextService>();
        services.AddScoped<ITenantContextService>(provider => provider.GetRequiredService<TenantContextService>());
        services.AddScoped<ICurrentTenant>(provider => provider.GetRequiredService<TenantContextService>());
        services.AddScoped<ITenantResolver, HeaderTenantResolver>();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.Configure<IpRestrictionOptions>(configuration.GetSection(IpRestrictionOptions.SectionName));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Clinic Management SaaS API",
                Version = "v1",
                Description = "Phase 1 foundation API for the Clinic Management SaaS modular monolith."
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }
                ] = []
            });
        });

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicyNames.SuperAdminOnly, policy => policy.RequireRole(ApplicationRoleNames.SuperAdmin))
            .AddPolicy(AuthorizationPolicyNames.ManageTenants, policy => policy.RequireClaim("permission", PermissionNames.ManageTenants))
            .AddPolicy(AuthorizationPolicyNames.ManageLocations, policy => policy.RequireClaim("permission", PermissionNames.ManageLocations))
            .AddPolicy(AuthorizationPolicyNames.ManageRoles, policy => policy.RequireClaim("permission", PermissionNames.ManageRoles))
            .AddPolicy(AuthorizationPolicyNames.ManagePatients, policy => policy.RequireClaim("permission", PermissionNames.ManagePatients))
            .AddPolicy(AuthorizationPolicyNames.ManageAppointments, policy => policy.RequireClaim("permission", PermissionNames.ManageAppointments))
            .AddPolicy(AuthorizationPolicyNames.ManageEncounters, policy => policy.RequireClaim("permission", PermissionNames.ManageEncounters))
            .AddPolicy(AuthorizationPolicyNames.ManageAI, policy => policy.RequireClaim("permission", PermissionNames.ManageAI))
            .AddPolicy(AuthorizationPolicyNames.ViewReports, policy => policy.RequireClaim("permission", PermissionNames.ViewReports))
            .AddPolicy(AuthorizationPolicyNames.ManageBilling, policy => policy.RequireClaim("permission", PermissionNames.ManageBilling));

        services.AddCors(options =>
        {
            options.AddPolicy(ApiCorsOptions.PolicyName, policy =>
            {
                var origins = configuration.GetSection(ApiCorsOptions.SectionName).Get<ApiCorsOptions>()?.AllowedOrigins ?? [];
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddHealthChecks();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
                limiterOptions.AutoReplenishment = true;
            });
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var key = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 300,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
        });

        return services;
    }
}
