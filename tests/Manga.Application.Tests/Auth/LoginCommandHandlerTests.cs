using Manga.Application.Auth.Commands.Login;
using Manga.Application.Common.Interfaces;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using NSubstitute;

namespace Manga.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IAuthSettings _authSettings = Substitute.For<IAuthSettings>();

    public LoginCommandHandlerTests()
    {
        _passwordHasher.Verify(Arg.Any<string>(), "valid_hash").Returns(true);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Is<string>(h => h != "valid_hash")).Returns(false);
        _tokenService.GenerateAccessToken(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>())
            .Returns(("access_token", "jti_123", DateTimeOffset.UtcNow.AddMinutes(15)));
        _tokenService.GenerateRefreshToken().Returns("raw_refresh_token");
        _authSettings.RefreshTokenExpirationDays.Returns(7);
    }

    private async Task<User> SeedUser(Infrastructure.Persistence.AppDbContext db)
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "valid_hash",
            EmailConfirmed = true,
        };
        db.Users.Add(user);
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Reader });
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResponse()
    {
        using var db = TestDbContextFactory.Create();
        await SeedUser(db);
        var handler = new LoginCommandHandler(db, _passwordHasher, _tokenService, _authSettings);

        var result = await handler.Handle(new LoginCommand("test@example.com", "password"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("access_token", result.Value.Auth.AccessToken);
        Assert.Equal("raw_refresh_token", result.Value.RawRefreshToken);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsFailure_GenericMessage()
    {
        using var db = TestDbContextFactory.Create();
        var user = await SeedUser(db);
        user.PasswordHash = "different_hash";
        await db.SaveChangesAsync();

        var handler = new LoginCommandHandler(db, _passwordHasher, _tokenService, _authSettings);
        var result = await handler.Handle(new LoginCommand("test@example.com", "wrong"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid credentials", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ReturnsFailure_GenericMessage()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new LoginCommandHandler(db, _passwordHasher, _tokenService, _authSettings);

        var result = await handler.Handle(new LoginCommand("nonexistent@example.com", "password"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid credentials", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_UpdatesLastLoginAt()
    {
        using var db = TestDbContextFactory.Create();
        var user = await SeedUser(db);
        var handler = new LoginCommandHandler(db, _passwordHasher, _tokenService, _authSettings);

        await handler.Handle(new LoginCommand("test@example.com", "password"), CancellationToken.None);

        var updatedUser = db.Users.First(u => u.Id == user.Id);
        Assert.NotNull(updatedUser.LastLoginAt);
    }

    [Fact]
    public async Task Handle_CreatesRefreshToken_WithFamily()
    {
        using var db = TestDbContextFactory.Create();
        await SeedUser(db);
        var handler = new LoginCommandHandler(db, _passwordHasher, _tokenService, _authSettings);

        await handler.Handle(new LoginCommand("test@example.com", "password"), CancellationToken.None);

        var refreshToken = db.RefreshTokens.FirstOrDefault();
        Assert.NotNull(refreshToken);
        Assert.NotEqual(Guid.Empty, refreshToken.Family);
    }
}
