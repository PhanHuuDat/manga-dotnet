using Manga.Application.Common.Interfaces;
using Manga.Domain.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Manga.Infrastructure.Services;

/// <summary>
/// Redis HyperLogLog-based unique view tracking.
/// Keys: "views:{TargetType}:{TargetId}:{yyyy-MM-dd}" with 30-day TTL.
/// Graceful degradation: returns 0 if Redis is unavailable.
/// </summary>
public class RedisViewTrackingService(
    IConnectionMultiplexer redis,
    ILogger<RedisViewTrackingService> logger) : IViewTrackingService
{
    private static readonly TimeSpan KeyTtl = TimeSpan.FromDays(30);

    public async Task<long> TrackAndGetUniqueCountAsync(
        ViewTargetType targetType, Guid targetId, DateOnly viewDate,
        string viewerIdentifier, CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            var key = $"views:{targetType}:{targetId}:{viewDate:yyyy-MM-dd}";

            var added = await db.HyperLogLogAddAsync(key, viewerIdentifier);

            // Set TTL only on first add to avoid resetting it on every view
            if (added)
                await db.KeyExpireAsync(key, KeyTtl, ExpireWhen.HasNoExpiry);

            return await db.HyperLogLogLengthAsync(key);
        }
        catch (Exception ex)
        {
            // Return -1 sentinel so the handler knows not to overwrite UniqueViewCount
            logger.LogWarning(ex, "Redis HyperLogLog unavailable for view tracking");
            return -1;
        }
    }
}
