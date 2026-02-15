using Manga.Application.Auth.DTOs;
using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginTokenResult>>;
