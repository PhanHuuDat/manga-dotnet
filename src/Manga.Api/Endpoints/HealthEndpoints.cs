using Manga.Infrastructure.Persistence;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

namespace Manga.Api.Endpoints;

/// <summary>
/// Health check endpoint for liveness/readiness probes.
/// </summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        // Liveness: simple health check (no dependencies)
        routes.MapGet("/health", () => TypedResults.Ok(new { Status = "Healthy" }))
            .WithName("HealthCheck")
            .WithTags("Health");

        // Readiness: check DB and Redis
        routes.MapGet("/health/ready", async (
            AppDbContext db,
            IConnectionMultiplexer redis,
            CancellationToken ct) =>
        {
            var checks = new Dictionary<string, string>();

            // Check database
            try
            {
                var canConnect = await db.Database.CanConnectAsync(ct);
                checks["database"] = canConnect ? "healthy" : "unhealthy";
            }
            catch
            {
                checks["database"] = "unhealthy";
            }

            // Check Redis
            try
            {
                var redisDb = redis.GetDatabase();
                await redisDb.PingAsync();
                checks["redis"] = "healthy";
            }
            catch
            {
                checks["redis"] = "unhealthy";
            }

            var allHealthy = checks.Values.All(v => v == "healthy");
            return allHealthy
                ? TypedResults.Ok(new { Status = "Ready", Checks = checks })
                : Results.Json(new { Status = "NotReady", Checks = checks }, statusCode: 503);
        })
        .WithName("ReadinessCheck")
        .WithTags("Health");
    }
}
