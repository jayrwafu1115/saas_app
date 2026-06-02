using Clinic.Api.Endpoints;
using Clinic.Api.Extensions;
using Clinic.Api.Options;
using Clinic.Application.DependencyInjection;
using Clinic.Infrastructure.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.WebHost.UseSentry();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

await app.Services.SeedIdentityAsync();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic Management SaaS API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors(ApiCorsOptions.PolicyName);
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").WithTags("Health");
app.MapFoundationEndpoints();
app.MapAuthEndpoints();
app.MapRoleEndpoints();
app.MapTenantEndpoints();
app.MapLocationEndpoints();

app.Run();

public partial class Program;
