using Manga.Application.Common.Interfaces;
using Manga.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Common.Services;

/// <summary>
/// Shared service for ownership and role-based authorization checks within handlers.
/// </summary>
public interface IUserAuthorizationService
{
    /// <summary>Returns true if the current user has Moderator or Admin role.</summary>
    Task<bool> HasModeratorPermissionAsync(CancellationToken ct = default);

    /// <summary>Returns true if the current user is the resource owner (by CreatedBy field).</summary>
    bool IsOwner(string? createdBy);
}

public class UserAuthorizationService(
    IAppDbContext db,
    ICurrentUserService currentUser) : IUserAuthorizationService
{
    public async Task<bool> HasModeratorPermissionAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return false;
        var userId = Guid.Parse(currentUser.UserId);
        var roles = await db.UserRoleMappings
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .ToListAsync(ct);
        return roles.Contains(UserRole.Moderator) || roles.Contains(UserRole.Admin);
    }

    public bool IsOwner(string? createdBy) =>
        !string.IsNullOrEmpty(currentUser.UserId) && createdBy == currentUser.UserId;
}
