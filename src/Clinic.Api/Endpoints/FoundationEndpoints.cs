namespace Clinic.Api.Endpoints;

public static class FoundationEndpoints
{
    public static IEndpointRouteBuilder MapFoundationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/foundation", () => Results.Ok(new
        {
            service = "Clinic Management SaaS API",
            phase = "Phase 1",
            status = "Foundation ready"
        }))
        .WithName("GetFoundationStatus")
        .WithTags("Foundation")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}
