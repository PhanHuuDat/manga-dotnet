using FluentValidation;
using FluentValidation.Results;
using Manga.Application.Auth.Commands.Register;
using Manga.Application.Common.Behaviors;
using Manga.Application.Common.Models;
using MediatR;
using NSubstitute;

namespace Manga.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var validators = Enumerable.Empty<IValidator<RegisterCommand>>();
        var behavior = new ValidationBehavior<RegisterCommand, Result>(validators);
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next(Arg.Any<CancellationToken>()).Returns(Result.Success());

        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "Password123!");
        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.True(result.Succeeded);
        await next.Received(1)(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPassingValidation_CallsNext()
    {
        var validator = new RegisterCommandValidator();
        var behavior = new ValidationBehavior<RegisterCommand, Result>(new[] { validator });
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next(Arg.Any<CancellationToken>()).Returns(Result.Success());

        var command = new RegisterCommand("testuser", "test@example.com", "Password123!", "Password123!");
        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.True(result.Succeeded);
        await next.Received(1)(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithFailingValidation_ThrowsValidationException()
    {
        var validator = new RegisterCommandValidator();
        var behavior = new ValidationBehavior<RegisterCommand, Result>(new[] { validator });
        var next = Substitute.For<RequestHandlerDelegate<Result>>();

        // Empty username and short password will fail validation
        var command = new RegisterCommand("", "bad-email", "short", "different");

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(command, next, CancellationToken.None));

        await next.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleValidationErrors_AggregatesAllErrors()
    {
        var validator = new RegisterCommandValidator();
        var behavior = new ValidationBehavior<RegisterCommand, Result>(new[] { validator });
        var next = Substitute.For<RequestHandlerDelegate<Result>>();

        // Empty fields trigger multiple errors
        var command = new RegisterCommand("", "", "", "mismatch");

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(command, next, CancellationToken.None));

        // Should have errors for username, email, password, and confirm password
        Assert.True(ex.Errors.Count() >= 3);
    }
}
