using Manga.Application.Auth.Commands.ResetPassword;
using Manga.Application.Common.Helpers;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using NSubstitute;

namespace Manga.Application.Tests.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    public ResetPasswordCommandHandlerTests()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("new_hashed_password");
    }

    private async Task<(User User, string RawToken)> SeedUserWithResetToken(
        Infrastructure.Persistence.AppDbContext db, bool expired = false, bool used = false)
    {
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "old_hash" };
        db.Users.Add(user);

        var rawToken = "reset_token_value";
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.PasswordReset,
            ExpiresAt = expired ? DateTimeOffset.UtcNow.AddHours(-1) : DateTimeOffset.UtcNow.AddHours(3),
            UsedAt = used ? DateTimeOffset.UtcNow.AddMinutes(-30) : null,
        });
        await db.SaveChangesAsync();
        return (user, rawToken);
    }

    [Fact]
    public async Task Handle_WithValidToken_UpdatesPassword()
    {
        using var db = TestDbContextFactory.Create();
        var (user, rawToken) = await SeedUserWithResetToken(db);
        var handler = new ResetPasswordCommandHandler(db, _passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand(rawToken, user.Id, "NewPassword123!", "NewPassword123!"), CancellationToken.None);

        Assert.True(result.Succeeded);
        var updatedUser = db.Users.First();
        Assert.Equal("new_hashed_password", updatedUser.PasswordHash);
    }

    [Fact]
    public async Task Handle_WithValidToken_RevokesAllRefreshTokens()
    {
        using var db = TestDbContextFactory.Create();
        var (user, rawToken) = await SeedUserWithResetToken(db);

        // Add active refresh tokens
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, Token = "hash1", Family = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        });
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, Token = "hash2", Family = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        });
        await db.SaveChangesAsync();

        var handler = new ResetPasswordCommandHandler(db, _passwordHasher);
        await handler.Handle(
            new ResetPasswordCommand(rawToken, user.Id, "NewPassword123!", "NewPassword123!"), CancellationToken.None);

        Assert.True(db.RefreshTokens.All(t => t.RevokedAt != null));
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var (user, rawToken) = await SeedUserWithResetToken(db, expired: true);
        var handler = new ResetPasswordCommandHandler(db, _passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand(rawToken, user.Id, "NewPassword123!", "NewPassword123!"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithUsedToken_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var (user, rawToken) = await SeedUserWithResetToken(db, used: true);
        var handler = new ResetPasswordCommandHandler(db, _passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand(rawToken, user.Id, "NewPassword123!", "NewPassword123!"), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithWrongUserId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var (_, rawToken) = await SeedUserWithResetToken(db);
        var handler = new ResetPasswordCommandHandler(db, _passwordHasher);

        var result = await handler.Handle(
            new ResetPasswordCommand(rawToken, Guid.NewGuid(), "NewPassword123!", "NewPassword123!"), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_SetsUsedAt_OnValidToken()
    {
        using var db = TestDbContextFactory.Create();
        var (user, rawToken) = await SeedUserWithResetToken(db);
        var handler = new ResetPasswordCommandHandler(db, _passwordHasher);

        await handler.Handle(
            new ResetPasswordCommand(rawToken, user.Id, "NewPassword123!", "NewPassword123!"), CancellationToken.None);

        var token = db.VerificationTokens.First();
        Assert.NotNull(token.UsedAt);
    }
}
