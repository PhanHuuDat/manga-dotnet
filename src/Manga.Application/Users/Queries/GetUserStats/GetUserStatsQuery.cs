using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Application.Users.DTOs;
using MediatR;

namespace Manga.Application.Users.Queries.GetUserStats;

[Authorize]
public record GetUserStatsQuery : IRequest<Result<UserStatsDto>>;
