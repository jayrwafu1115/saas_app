using Clinic.Application.Billing.Commands;
using Clinic.Application.Billing.Queries;
using Clinic.Application.Common.Security;
using Clinic.Domain.Billing;
using FluentValidation;
using MediatR;

namespace Clinic.Api.Endpoints;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing")
            .RequireAuthorization(AuthorizationPolicyNames.ManageBilling)
            .WithTags("Billing");

        group.MapGet("/plans", async (ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetSubscriptionPlansQuery(), cancellationToken)))
            .WithName("GetSubscriptionPlans")
            .WithSummary("List SaaS subscription plans")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/trial", async (StartTrialCommand command, ISender sender, CancellationToken cancellationToken) =>
            await SendCommand(command, sender, cancellationToken))
            .WithName("StartSubscriptionTrial")
            .WithSummary("Start or change a tenant trial")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/checkout", async (CreateCheckoutRequest request, ISender sender, CancellationToken cancellationToken) =>
            await SendCommand(new CreateCheckoutCommand(request.TenantId, request.PlanCode, request.Provider), sender, cancellationToken))
            .WithName("CreateBillingCheckout")
            .WithSummary("Create a GCash or Maya checkout")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/overview", async (Guid? tenantId, ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetSubscriptionOverviewQuery(tenantId), cancellationToken)))
            .WithName("GetSubscriptionOverview")
            .WithSummary("Get subscription overview and tenant usage")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/tenants/{tenantId:guid}/restriction", async (Guid tenantId, ISender sender, CancellationToken cancellationToken) =>
            Results.Ok(await sender.Send(new GetTenantRestrictionQuery(tenantId), cancellationToken)))
            .WithName("GetTenantRestriction")
            .WithSummary("Get tenant subscription restriction status")
            .Produces(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> SendCommand<TResponse>(IRequest<TResponse> command, ISender sender, CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await sender.Send(command, cancellationToken));
        }
        catch (ValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
        }
        catch (KeyNotFoundException exception)
        {
            return Results.NotFound(new { message = exception.Message });
        }
    }

    private sealed record CreateCheckoutRequest(Guid TenantId, string PlanCode, BillingProvider Provider);
}
