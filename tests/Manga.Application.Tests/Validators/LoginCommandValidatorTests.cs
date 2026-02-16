using FluentValidation.TestHelper;
using Manga.Application.Auth.Commands.Login;

namespace Manga.Application.Tests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var command = new LoginCommand("test@example.com", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Email_Empty_Fails(string? email)
    {
        var command = new LoginCommand(email!, "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Password_Empty_Fails(string? password)
    {
        var command = new LoginCommand("test@example.com", password!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }
}
