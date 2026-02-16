using Manga.Application.Auth.Queries.GetCurrentUser;
using Manga.Application.Common.Interfaces;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using NSubstitute;

namespace Manga.Application.Tests.Auth;

public class GetCurrentUserQueryHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    [Fact]
    public async Task Handle_WithValidUser_ReturnsProfile()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            DisplayName = "Test User",
            Level = 5,
            EmailConfirmed = true,
        };
        db.Users.Add(user);
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Reader });
        await db.SaveChangesAsync();

        _currentUser.UserId.Returns(user.Id.ToString());
        var handler = new GetCurrentUserQueryHandler(db, _currentUser);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("testuser", result.Value.Username);
        Assert.Equal("test@example.com", result.Value.Email);
        Assert.Equal("Test User", result.Value.DisplayName);
        Assert.Equal(5, result.Value.Level);
        Assert.True(result.Value.EmailConfirmed);
        Assert.Contains("Reader", result.Value.Roles);
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        _currentUser.UserId.Returns("not-a-guid");
        var handler = new GetCurrentUserQueryHandler(db, _currentUser);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("authenticated", result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        _currentUser.UserId.Returns(Guid.NewGuid().ToString());
        var handler = new GetCurrentUserQueryHandler(db, _currentUser);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WithMultipleRoles_ReturnsAllRoles()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "admin", Email = "admin@example.com", PasswordHash = "hash" };
        db.Users.Add(user);
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Reader });
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Admin });
        await db.SaveChangesAsync();

        _currentUser.UserId.Returns(user.Id.ToString());
        var handler = new GetCurrentUserQueryHandler(db, _currentUser);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Roles.Count);
        Assert.Contains("Reader", result.Value.Roles);
        Assert.Contains("Admin", result.Value.Roles);
    }

    [Fact]
    public async Task Handle_WithNullUserId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        _currentUser.UserId.Returns((string?)null);
        var handler = new GetCurrentUserQueryHandler(db, _currentUser);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.False(result.Succeeded);
    }
}
