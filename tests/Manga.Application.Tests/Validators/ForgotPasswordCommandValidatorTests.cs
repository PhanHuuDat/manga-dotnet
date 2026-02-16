using FluentValidation.TestHelper;
using Manga.Application.Auth.Commands.ForgotPassword;

namespace Manga.Application.Tests.Validators;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var command = new ForgotPasswordCommand("test@example.com");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Email_Empty_Fails(string? email)
    {
        var command = new ForgotPasswordCommand(email!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_InvalidFormat_Fails()
    {
        var command = new ForgotPasswordCommand("not-an-email");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format.");
    }
}
