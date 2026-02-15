namespace Manga.Domain.Interfaces;

/// <summary>
/// Token generation contract. Implementation in Infrastructure (JWT + RandomNumberGenerator).
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT access token with claims.</summary>
    (string Token, string Jti, DateTimeOffset ExpiresAt) GenerateAccessToken(
        Guid userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);

    /// <summary>Generates a cryptographically random refresh token (base64).</summary>
    string GenerateRefreshToken();

    /// <summary>Generates a URL-safe verification token for email/password reset.</summary>
    string GenerateEmailToken();
}
