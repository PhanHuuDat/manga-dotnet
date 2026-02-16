using FluentValidation.TestHelper;
using Manga.Application.Auth.Commands.ResetPassword;

namespace Manga.Application.Tests.Validators;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var command = new ResetPasswordCommand("token", Guid.NewGuid(), "NewPassword123!", "NewPassword123!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void NewPassword_Empty_Fails(string? password)
    {
        var command = new ResetPasswordCommand("token", Guid.NewGuid(), password!, password!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_TooShort_Fails()
    {
        var command = new ResetPasswordCommand("token", Guid.NewGuid(), "Short1!", "Short1!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void ConfirmPassword_Mismatch_Fails()
    {
        var command = new ResetPasswordCommand("token", Guid.NewGuid(), "NewPassword123!", "DifferentPass!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match.");
    }
}
