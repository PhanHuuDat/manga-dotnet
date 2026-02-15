using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token, Guid UserId) : IRequest<Result>;
