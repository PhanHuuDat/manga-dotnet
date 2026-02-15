using Manga.Domain.Constants;
using Manga.Domain.Enums;

namespace Manga.Domain.Tests.Constants;

public class RolePermissionsTests
{
    [Fact]
    public void AdminRole_HasAllPermissions()
    {
        var allPermissions = Enum.GetValues<Permission>();
        var adminPermissions = RolePermissions.GetPermissions(UserRole.Admin);

        foreach (var permission in allPermissions)
        {
            Assert.Contains(permission, adminPermissions);
        }
    }

    [Fact]
    public void ReaderRole_HasLimitedPermissions()
    {
        var permissions = RolePermissions.GetPermissions(UserRole.Reader);

        Assert.Contains(Permission.CommentCreate, permissions);
        Assert.Contains(Permission.CommentUpdate, permissions);
        Assert.Contains(Permission.AttachmentUpload, permissions);
        Assert.DoesNotContain(Permission.MangaCreate, permissions);
        Assert.DoesNotContain(Permission.MangaDelete, permissions);
    }

    [Fact]
    public void UploaderRole_HasMangaAndChapterCreate()
    {
        var permissions = RolePermissions.GetPermissions(UserRole.Uploader);

        Assert.Contains(Permission.MangaCreate, permissions);
        Assert.Contains(Permission.MangaUpdate, permissions);
        Assert.Contains(Permission.ChapterCreate, permissions);
        Assert.Contains(Permission.ChapterUpdate, permissions);
    }

    [Fact]
    public void AllPermissions_AreMappedToAtLeastOneRole()
    {
        var allPermissions = Enum.GetValues<Permission>();
        var allRoles = Enum.GetValues<UserRole>();

        foreach (var permission in allPermissions)
        {
            var isMapped = allRoles.Any(role =>
                RolePermissions.GetPermissions(role).Contains(permission));
            Assert.True(isMapped, $"Permission {permission} is not mapped to any role");
        }
    }

    [Fact]
    public void GetPermissions_WithMultipleRoles_ReturnsDeduplicatedUnion()
    {
        var roles = new[] { UserRole.Reader, UserRole.Uploader };
        var combined = RolePermissions.GetPermissions(roles);
        var distinct = combined.Distinct().ToList();

        Assert.Equal(combined.Count, distinct.Count);
        Assert.Contains(Permission.CommentCreate, combined);
        Assert.Contains(Permission.MangaCreate, combined);
    }
}
