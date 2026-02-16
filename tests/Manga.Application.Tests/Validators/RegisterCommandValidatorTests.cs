using FluentValidation.TestHelper;
using Manga.Application.Auth.Commands.Register;

namespace Manga.Application.Tests.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_PassesValidation()
    {
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Username_Empty_Fails(string? username)
    {
        var command = new RegisterCommand(username!, "test@example.com", "Password123!", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_TooShort_Fails()
    {
        var command = new RegisterCommand("ab", "test@example.com", "Password123!", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username must be at least 3 characters.");
    }

    [Fact]
    public void Username_TooLong_Fails()
    {
        var command = new RegisterCommand(new string('a', 51), "test@example.com", "Password123!", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username must not exceed 50 characters.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Email_Empty_Fails(string? email)
    {
        var command = new RegisterCommand("testuser", email!, "Password123!", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_InvalidFormat_Fails()
    {
        var command = new RegisterCommand("testuser", "not-an-email", "Password123!", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Password_Empty_Fails(string? password)
    {
        var command = new RegisterCommand("testuser", "test@example.com", password!, password!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_TooShort_Fails()
    {
        var command = new RegisterCommand("testuser", "test@example.com", "Short1!", "Short1!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void ConfirmPassword_Mismatch_Fails()
    {
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "DifferentPass!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match.");
    }
}
