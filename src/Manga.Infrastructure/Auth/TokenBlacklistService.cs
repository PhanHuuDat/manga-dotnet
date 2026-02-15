using Manga.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Manga.Infrastructure.Auth;

/// <summary>
/// Redis-backed JWT jti blacklist. Keys auto-expire with token TTL.
/// </summary>
public class TokenBlacklistService(IConnectionMultiplexer redis) : ITokenBlacklistService
{
    private const string Prefix = "blacklist:";

    public async Task BlacklistAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        await db.StringSetAsync($"{Prefix}{jti}", "1", ttl);
    }

    public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        return await db.KeyExistsAsync($"{Prefix}{jti}");
    }
}
