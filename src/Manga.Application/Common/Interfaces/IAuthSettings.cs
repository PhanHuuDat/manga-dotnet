namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Auth configuration abstraction so Application layer doesn't depend on Infrastructure.
/// </summary>
public interface IAuthSettings
{
    int RefreshTokenExpirationDays { get; }
}
