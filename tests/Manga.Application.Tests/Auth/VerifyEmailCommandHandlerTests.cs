using Manga.Application.Auth.Commands.VerifyEmail;
using Manga.Application.Common.Helpers;
using Manga.Domain.Entities;
using Manga.Domain.Enums;

namespace Manga.Application.Tests.Auth;

public class VerifyEmailCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidToken_ConfirmsEmail()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash", EmailConfirmed = false };
        db.Users.Add(user);

        var rawToken = "valid_token";
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.EmailVerification,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
        });
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db);
        var result = await handler.Handle(new VerifyEmailCommand(rawToken, user.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(db.Users.First().EmailConfirmed);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db);
        var result = await handler.Handle(new VerifyEmailCommand("wrong_token", user.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        db.Users.Add(user);

        var rawToken = "expired_token";
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.EmailVerification,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1),
        });
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db);
        var result = await handler.Handle(new VerifyEmailCommand(rawToken, user.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithAlreadyUsedToken_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        db.Users.Add(user);

        var rawToken = "used_token";
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.EmailVerification,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            UsedAt = DateTimeOffset.UtcNow.AddHours(-1),
        });
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db);
        var result = await handler.Handle(new VerifyEmailCommand(rawToken, user.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WithWrongUserId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        db.Users.Add(user);

        var rawToken = "valid_token";
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.EmailVerification,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
        });
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db);
        var result = await handler.Handle(new VerifyEmailCommand(rawToken, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_SetsUsedAt_OnValidToken()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        db.Users.Add(user);

        var rawToken = "valid_token";
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.EmailVerification,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
        });
        await db.SaveChangesAsync();

        var handler = new VerifyEmailCommandHandler(db);
        await handler.Handle(new VerifyEmailCommand(rawToken, user.Id), CancellationToken.None);

        var token = db.VerificationTokens.First();
        Assert.NotNull(token.UsedAt);
    }
}
