using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Auth.Commands.Logout;

[Authorize]
public record LogoutCommand(string Jti, DateTimeOffset AccessTokenExpiry) : IRequest<Result>;
