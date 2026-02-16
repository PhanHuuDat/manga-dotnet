using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Users.Commands.UpdateProfile;

[Authorize]
public record UpdateProfileCommand(string? DisplayName) : IRequest<Result>;
