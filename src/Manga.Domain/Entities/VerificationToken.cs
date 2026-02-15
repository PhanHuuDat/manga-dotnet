using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

/// <summary>
/// Unified token for email verification and password reset. Stored as SHA-256 hash.
/// </summary>
public class VerificationToken : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the raw token.</summary>
    public string Token { get; set; } = string.Empty;

    public VerificationTokenType TokenType { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsUsed => UsedAt is not null;
    public bool IsValid => !IsExpired && !IsUsed;
}
