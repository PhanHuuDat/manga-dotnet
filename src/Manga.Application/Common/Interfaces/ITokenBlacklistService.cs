namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Redis-backed JWT blacklist for logout invalidation.
/// </summary>
public interface ITokenBlacklistService
{
    Task BlacklistAsync(string jti, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default);
}
