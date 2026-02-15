using System.Security.Claims;
using Manga.Application.Common.Interfaces;

namespace Manga.Api.Services;

/// <summary>
/// Extracts current user from HttpContext claims.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
