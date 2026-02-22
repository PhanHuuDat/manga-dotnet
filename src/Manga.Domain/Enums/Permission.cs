namespace Manga.Domain.Enums;

/// <summary>
/// Granular permissions for RBAC. Gap numbering allows adding within groups.
/// </summary>
public enum Permission
{
    // Manga
    MangaCreate = 100,
    MangaUpdate = 101,
    MangaDelete = 102,

    // Chapter
    ChapterCreate = 200,
    ChapterUpdate = 201,
    ChapterDelete = 202,

    // Comment
    CommentCreate = 300,
    CommentUpdate = 301,
    CommentDelete = 302,
    CommentModerate = 303,

    // User
    UserViewAll = 400,
    UserUpdate = 401,
    UserDelete = 402,
    UserManageRoles = 403,

    // Upload
    AttachmentUpload = 500,
    AttachmentDelete = 501,

    // Genre
    GenreCreate = 600,
    GenreUpdate = 601,
    GenreDelete = 602,

    // Admin
    AdminViewStats = 700,
    AdminManageUsers = 701,
    AdminManageComments = 702,
}
