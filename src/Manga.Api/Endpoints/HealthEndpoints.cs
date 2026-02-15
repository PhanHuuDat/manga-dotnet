namespace Manga.Api.Endpoints;

/// <summary>
/// Health check endpoint for liveness/readiness probes.
/// </summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/health", () => TypedResults.Ok(new { Status = "Healthy" }))
            .WithName("HealthCheck")
            .WithTags("Health");
    }
}
