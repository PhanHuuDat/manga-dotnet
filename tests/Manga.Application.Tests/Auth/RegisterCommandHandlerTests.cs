using Manga.Application.Auth.Commands.Register;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using NSubstitute;

namespace Manga.Application.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();

    public RegisterCommandHandlerTests()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_password");
        _tokenService.GenerateEmailToken().Returns("test_email_token");
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesUserWithHashedPassword()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new RegisterCommandHandler(db, _passwordHasher, _tokenService, _emailService);
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "Password123!");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        var user = db.Users.FirstOrDefault(u => u.Email == "test@example.com");
        Assert.NotNull(user);
        Assert.Equal("hashed_password", user.PasswordHash);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Username = "existing", Email = "test@example.com", PasswordHash = "hash" });
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db, _passwordHasher, _tokenService, _emailService);
        var command = new RegisterCommand("newuser", "test@example.com", "Password123!", "Password123!");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Email", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithDuplicateUsername_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Username = "testuser", Email = "existing@example.com", PasswordHash = "hash" });
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db, _passwordHasher, _tokenService, _emailService);
        var command = new RegisterCommand("testuser", "new@example.com", "Password123!", "Password123!");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Username", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_AssignsReaderRole_ByDefault()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new RegisterCommandHandler(db, _passwordHasher, _tokenService, _emailService);
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "Password123!");

        await handler.Handle(command, CancellationToken.None);

        var roleMapping = db.UserRoleMappings.FirstOrDefault();
        Assert.NotNull(roleMapping);
        Assert.Equal(UserRole.Reader, roleMapping.Role);
    }

    [Fact]
    public async Task Handle_SendsVerificationEmail()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new RegisterCommandHandler(db, _passwordHasher, _tokenService, _emailService);
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "Password123!");

        await handler.Handle(command, CancellationToken.None);

        await _emailService.Received(1).SendEmailVerificationAsync(
            "test@example.com", "testuser", "test_email_token", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SetsEmailConfirmed_ToFalse()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new RegisterCommandHandler(db, _passwordHasher, _tokenService, _emailService);
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "Password123!");

        await handler.Handle(command, CancellationToken.None);

        var user = db.Users.First();
        Assert.False(user.EmailConfirmed);
    }
}
