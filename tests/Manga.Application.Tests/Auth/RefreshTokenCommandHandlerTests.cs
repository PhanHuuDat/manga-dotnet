using Manga.Application.Auth.Commands.RefreshToken;
using Manga.Application.Common.Helpers;
using Manga.Application.Common.Interfaces;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using NSubstitute;

namespace Manga.Application.Tests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IAuthSettings _authSettings = Substitute.For<IAuthSettings>();

    public RefreshTokenCommandHandlerTests()
    {
        _tokenService.GenerateAccessToken(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>())
            .Returns(("new_access_token", "new_jti", DateTimeOffset.UtcNow.AddMinutes(15)));
        _tokenService.GenerateRefreshToken().Returns("new_raw_refresh");
        _authSettings.RefreshTokenExpirationDays.Returns(7);
    }

    private async Task<(User User, string RawToken)> SeedUserWithRefreshToken(
        Infrastructure.Persistence.AppDbContext db, bool revoked = false, bool expired = false)
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            EmailConfirmed = true,
        };
        db.Users.Add(user);
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Reader });

        var rawToken = "raw_refresh_token_value";
        var token = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            Family = Guid.NewGuid(),
            ExpiresAt = expired ? DateTimeOffset.UtcNow.AddDays(-1) : DateTimeOffset.UtcNow.AddDays(7),
            RevokedAt = revoked ? DateTimeOffset.UtcNow.AddHours(-1) : null,
        };
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        return (user, rawToken);
    }

    [Fact]
    public async Task Handle_WithValidToken_ReturnsNewTokens()
    {
        using var db = TestDbContextFactory.Create();
        var (_, rawToken) = await SeedUserWithRefreshToken(db);
        var handler = new RefreshTokenCommandHandler(db, _tokenService, _authSettings);

        var result = await handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("new_access_token", result.Value!.Auth.AccessToken);
        Assert.Equal("new_raw_refresh", result.Value.RawRefreshToken);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var (_, rawToken) = await SeedUserWithRefreshToken(db, expired: true);
        var handler = new RefreshTokenCommandHandler(db, _tokenService, _authSettings);

        var result = await handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("expired", result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WithRevokedToken_RevokesEntireFamily()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            EmailConfirmed = true,
        };
        db.Users.Add(user);
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Reader });

        var family = Guid.NewGuid();
        var revokedRawToken = "revoked_token";

        // Revoked token (reuse attempt)
        db.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(revokedRawToken),
            Family = family,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            RevokedAt = DateTimeOffset.UtcNow.AddHours(-1),
        });
        // Active sibling in same family
        db.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash("active_sibling"),
            Family = family,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        });
        await db.SaveChangesAsync();

        var handler = new RefreshTokenCommandHandler(db, _tokenService, _authSettings);
        var result = await handler.Handle(new RefreshTokenCommand(revokedRawToken), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("reuse", result.Errors[0], StringComparison.OrdinalIgnoreCase);
        // All family tokens should be revoked
        Assert.True(db.RefreshTokens.All(t => t.RevokedAt != null));
    }

    [Fact]
    public async Task Handle_RotatesToken_SetsReplacedByTokenId()
    {
        using var db = TestDbContextFactory.Create();
        var (_, rawToken) = await SeedUserWithRefreshToken(db);
        var handler = new RefreshTokenCommandHandler(db, _tokenService, _authSettings);

        await handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        var originalToken = db.RefreshTokens.First(t => t.Token == TokenHasher.Hash(rawToken));
        Assert.NotNull(originalToken.RevokedAt);
        Assert.NotNull(originalToken.ReplacedByTokenId);
    }
}
