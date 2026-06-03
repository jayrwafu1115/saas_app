using Clinic.Application.Common.Interfaces;
using Clinic.Application.AI;
using Clinic.Infrastructure.AI;
using Clinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Clinic.Tests.Api;

internal sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"clinic-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedData:SuperAdmin:Email"] = "superadmin@test.local",
                ["SeedData:SuperAdmin:Password"] = "SuperAdmin123!",
                ["SeedData:SuperAdmin:DisplayName"] = "Super Admin",
                ["ConnectionStrings:Redis"] = string.Empty
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.RemoveAll<IObjectStorageService>();
            services.AddSingleton<IObjectStorageService, FakeObjectStorageService>();
            services.RemoveAll<IConnectionMultiplexer>();
            services.RemoveAll<IAIResponseCache>();
            services.AddMemoryCache();
            services.AddSingleton<IAIResponseCache, InMemoryAIResponseCache>();
        });
    }

    private sealed class FakeObjectStorageService : IObjectStorageService
    {
        public Task<StoredObject> UploadAsync(
            string objectKey,
            Stream content,
            long contentLength,
            string contentType,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new StoredObject(objectKey));
        }
    }
}
