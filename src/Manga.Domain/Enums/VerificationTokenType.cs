namespace Manga.Domain.Enums;

/// <summary>
/// Differentiates verification token purposes within the unified VerificationToken entity.
/// </summary>
public enum VerificationTokenType
{
    EmailVerification = 0,
    PasswordReset = 1,
}
