using Clinic.Application.Common.Interfaces;
using Clinic.Application.Locations;
using Clinic.Application.Patients;
using Clinic.Application.Tenants;
using Clinic.Domain.Users;
using Clinic.Infrastructure.Identity;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Persistence.Repositories;
using Clinic.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Clinic.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.Configure<MinioStorageOptions>(options =>
        {
            var section = configuration.GetSection(MinioStorageOptions.SectionName);
            options.Endpoint = section["Endpoint"] ?? options.Endpoint;
            options.AccessKey = section["AccessKey"] ?? options.AccessKey;
            options.SecretKey = section["SecretKey"] ?? options.SecretKey;
            options.BucketName = section["BucketName"] ?? options.BucketName;
            if (bool.TryParse(section["UseSsl"], out var useSsl))
            {
                options.UseSsl = useSsl;
            }
        });
        services.AddScoped<IObjectStorageService, MinioObjectStorageService>();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        }

        return services;
    }
}
