using Manga.Application.Auth.Commands.ForgotPassword;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using NSubstitute;

namespace Manga.Application.Tests.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();

    public ForgotPasswordCommandHandlerTests()
    {
        _tokenService.GenerateEmailToken().Returns("reset_token");
    }

    [Fact]
    public async Task Handle_WithExistingUser_CreatesTokenAndSendsEmail()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ForgotPasswordCommandHandler(db, _tokenService, _emailService);
        var result = await handler.Handle(new ForgotPasswordCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(db.VerificationTokens);
        var token = db.VerificationTokens.First();
        Assert.Equal(VerificationTokenType.PasswordReset, token.TokenType);
        Assert.Equal(user.Id, token.UserId);
    }

    [Fact]
    public async Task Handle_WithExistingUser_SendsPasswordResetEmail()
    {
        using var db = TestDbContextFactory.Create();
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ForgotPasswordCommandHandler(db, _tokenService, _emailService);
        await handler.Handle(new ForgotPasswordCommand("test@example.com"), CancellationToken.None);

        await _emailService.Received(1).SendPasswordResetAsync(
            "test@example.com", "testuser", "reset_token", user.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ReturnsSuccess_PreventsEnumeration()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new ForgotPasswordCommandHandler(db, _tokenService, _emailService);

        var result = await handler.Handle(new ForgotPasswordCommand("nonexistent@example.com"), CancellationToken.None);

        Assert.True(result.Succeeded);
        await _emailService.DidNotReceive().SendPasswordResetAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreatesToken_WithFutureExpiry()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hash" });
        await db.SaveChangesAsync();

        var handler = new ForgotPasswordCommandHandler(db, _tokenService, _emailService);
        await handler.Handle(new ForgotPasswordCommand("test@example.com"), CancellationToken.None);

        var token = db.VerificationTokens.First();
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);
    }
}
