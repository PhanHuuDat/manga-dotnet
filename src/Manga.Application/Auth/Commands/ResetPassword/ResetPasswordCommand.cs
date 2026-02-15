using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Token,
    Guid UserId,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result>;
