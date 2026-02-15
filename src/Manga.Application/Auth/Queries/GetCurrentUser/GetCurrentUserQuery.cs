using Manga.Application.Auth.DTOs;
using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Auth.Queries.GetCurrentUser;

[Authorize]
public record GetCurrentUserQuery : IRequest<Result<UserProfileResponse>>;
