using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
