using Manga.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Manga.Infrastructure.Services;

/// <summary>
/// Wraps IConnectionMultiplexer to provide IDatabase access for direct Redis commands.
/// </summary>
public class RedisConnectionProvider(IConnectionMultiplexer connectionMultiplexer) : IRedisConnectionProvider
{
    public IDatabase GetDatabase() => connectionMultiplexer.GetDatabase();
}
