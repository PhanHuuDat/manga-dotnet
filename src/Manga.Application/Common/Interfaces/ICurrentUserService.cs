namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Provides current authenticated user information.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
}
