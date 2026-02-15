using StackExchange.Redis;

namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Provides access to the Redis connection multiplexer for direct Redis operations
/// (HINCRBY, PFADD, etc.) beyond simple IDistributedCache usage.
/// </summary>
public interface IRedisConnectionProvider
{
    /// <summary>Gets the Redis database instance for direct commands.</summary>
    IDatabase GetDatabase();
}
