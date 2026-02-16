using Manga.Application.Common.Behaviors;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Exceptions;
using MediatR;
using NSubstitute;

namespace Manga.Application.Tests.Behaviors;

public class AuthorizationBehaviorTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    // Unauthenticated request (no [Authorize])
    private record PublicRequest : IRequest<Result>;

    // Authenticated request
    [Authorize]
    private record AuthenticatedRequest : IRequest<Result>;

    // Permission-gated request
    [RequirePermission(nameof(Permission.MangaCreate))]
    private record PermissionRequest : IRequest<Result>;

    [Fact]
    public async Task Handle_WithoutAuthorizeAttribute_CallsNext()
    {
        using var db = Auth.TestDbContextFactory.Create();
        var behavior = new AuthorizationBehavior<PublicRequest, Result>(_currentUser, db);
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next(Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await behavior.Handle(new PublicRequest(), next, CancellationToken.None);

        Assert.True(result.Succeeded);
        await next.Received(1)(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAuthorize_UnauthenticatedUser_ThrowsUnauthorized()
    {
        using var db = Auth.TestDbContextFactory.Create();
        _currentUser.UserId.Returns((string?)null);
        var behavior = new AuthorizationBehavior<AuthenticatedRequest, Result>(_currentUser, db);
        var next = Substitute.For<RequestHandlerDelegate<Result>>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => behavior.Handle(new AuthenticatedRequest(), next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAuthorize_AuthenticatedUser_CallsNext()
    {
        using var db = Auth.TestDbContextFactory.Create();
        _currentUser.UserId.Returns(Guid.NewGuid().ToString());
        var behavior = new AuthorizationBehavior<AuthenticatedRequest, Result>(_currentUser, db);
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next(Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await behavior.Handle(new AuthenticatedRequest(), next, CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithRequirePermission_UserHasPermission_CallsNext()
    {
        using var db = Auth.TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId.ToString());

        // Uploader has MangaCreate permission
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = userId, Role = UserRole.Uploader });
        await db.SaveChangesAsync();

        var behavior = new AuthorizationBehavior<PermissionRequest, Result>(_currentUser, db);
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next(Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await behavior.Handle(new PermissionRequest(), next, CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithRequirePermission_UserLacksPermission_ThrowsForbidden()
    {
        using var db = Auth.TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId.ToString());

        // Reader does NOT have MangaCreate permission
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = userId, Role = UserRole.Reader });
        await db.SaveChangesAsync();

        var behavior = new AuthorizationBehavior<PermissionRequest, Result>(_currentUser, db);
        var next = Substitute.For<RequestHandlerDelegate<Result>>();

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => behavior.Handle(new PermissionRequest(), next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithRequirePermission_AdminUser_CallsNext()
    {
        using var db = Auth.TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId.ToString());

        // Admin has ALL permissions
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = userId, Role = UserRole.Admin });
        await db.SaveChangesAsync();

        var behavior = new AuthorizationBehavior<PermissionRequest, Result>(_currentUser, db);
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next(Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await behavior.Handle(new PermissionRequest(), next, CancellationToken.None);

        Assert.True(result.Succeeded);
    }
}
