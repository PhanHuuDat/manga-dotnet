using Manga.Domain.Enums;

namespace Manga.Domain.Constants;

/// <summary>
/// Static mapping of UserRole to Permission sets. Database-driven planned for future.
/// </summary>
public static class RolePermissions
{
    private static readonly Dictionary<UserRole, Permission[]> Mappings = new()
    {
        [UserRole.Reader] =
        [
            Permission.CommentCreate,
            Permission.CommentUpdate,
            Permission.AttachmentUpload,
        ],
        [UserRole.Uploader] =
        [
            Permission.CommentCreate,
            Permission.CommentUpdate,
            Permission.AttachmentUpload,
            Permission.MangaCreate,
            Permission.MangaUpdate,
            Permission.ChapterCreate,
            Permission.ChapterUpdate,
            Permission.AttachmentDelete,
            Permission.AdminViewStats,
        ],
        [UserRole.Moderator] =
        [
            Permission.CommentCreate,
            Permission.CommentUpdate,
            Permission.CommentDelete,
            Permission.CommentModerate,
            Permission.MangaCreate,
            Permission.MangaUpdate,
            Permission.MangaDelete,
            Permission.ChapterCreate,
            Permission.ChapterUpdate,
            Permission.ChapterDelete,
            Permission.AttachmentUpload,
            Permission.AttachmentDelete,
            Permission.AdminViewStats,
            Permission.AdminManageComments,
        ],
        [UserRole.Admin] = Enum.GetValues<Permission>(),
    };

    /// <summary>Gets all permissions for a single role.</summary>
    public static IReadOnlyList<Permission> GetPermissions(UserRole role) =>
        Mappings.TryGetValue(role, out var permissions) ? permissions : [];

    /// <summary>Gets combined permissions for multiple roles (deduplicated).</summary>
    public static IReadOnlyList<Permission> GetPermissions(IEnumerable<UserRole> roles) =>
        roles.SelectMany(GetPermissions).Distinct().ToList();
}
