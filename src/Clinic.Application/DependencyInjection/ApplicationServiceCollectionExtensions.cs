using FluentValidation;
using Clinic.Application.AI;
using Clinic.Application.Common.Behaviors;
using Clinic.Application.Appointments;
using Clinic.Application.Billing;
using Clinic.Application.Reporting;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IAIService, AIService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IBillingService, BillingService>();

        return services;
    }
}
