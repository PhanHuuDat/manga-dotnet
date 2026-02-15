using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// Refresh token for JWT rotation. Stored as SHA-256 hash. Uses family tracking for reuse detection.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the raw token.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Token family for rotation reuse detection. All rotated tokens share the same family.</summary>
    public Guid Family { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>Points to the token that replaced this one during rotation.</summary>
    public Guid? ReplacedByTokenId { get; set; }

    public string? CreatedByIp { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsExpired && !IsRevoked;
}
