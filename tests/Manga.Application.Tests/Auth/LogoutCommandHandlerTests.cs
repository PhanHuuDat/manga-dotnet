using Manga.Application.Auth.Commands.Logout;
using Manga.Application.Common.Interfaces;
using Manga.Domain.Entities;
using NSubstitute;

namespace Manga.Application.Tests.Auth;

public class LogoutCommandHandlerTests
{
    private readonly ITokenBlacklistService _blacklist = Substitute.For<ITokenBlacklistService>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    [Fact]
    public async Task Handle_BlacklistsAccessToken_WhenNotExpired()
    {
        using var db = TestDbContextFactory.Create();
        _currentUser.UserId.Returns(Guid.NewGuid().ToString());
        var handler = new LogoutCommandHandler(db, _blacklist, _currentUser);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(10);

        await handler.Handle(new LogoutCommand("jti_123", expiry), CancellationToken.None);

        await _blacklist.Received(1).BlacklistAsync("jti_123", Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotBlacklist_WhenTokenAlreadyExpired()
    {
        using var db = TestDbContextFactory.Create();
        _currentUser.UserId.Returns(Guid.NewGuid().ToString());
        var handler = new LogoutCommandHandler(db, _blacklist, _currentUser);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(-5);

        await handler.Handle(new LogoutCommand("jti_123", expiry), CancellationToken.None);

        await _blacklist.DidNotReceive().BlacklistAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RevokesAllRefreshTokens_ForUser()
    {
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId.ToString());

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId, Token = "hash1", Family = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        });
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId, Token = "hash2", Family = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        });
        await db.SaveChangesAsync();

        var handler = new LogoutCommandHandler(db, _blacklist, _currentUser);
        var result = await handler.Handle(
            new LogoutCommand("jti_123", DateTimeOffset.UtcNow.AddMinutes(10)), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(db.RefreshTokens.All(t => t.RevokedAt != null));
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_StillSucceeds_SkipsTokenRevocation()
    {
        using var db = TestDbContextFactory.Create();
        _currentUser.UserId.Returns("not-a-guid");
        var handler = new LogoutCommandHandler(db, _blacklist, _currentUser);

        var result = await handler.Handle(
            new LogoutCommand("jti_123", DateTimeOffset.UtcNow.AddMinutes(10)), CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_DoesNotRevokeOtherUsers_RefreshTokens()
    {
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId.ToString());

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId, Token = "hash1", Family = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        });
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = otherUserId, Token = "hash2", Family = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        });
        await db.SaveChangesAsync();

        var handler = new LogoutCommandHandler(db, _blacklist, _currentUser);
        await handler.Handle(
            new LogoutCommand("jti_123", DateTimeOffset.UtcNow.AddMinutes(10)), CancellationToken.None);

        var otherToken = db.RefreshTokens.First(t => t.UserId == otherUserId);
        Assert.Null(otherToken.RevokedAt);
    }
}
