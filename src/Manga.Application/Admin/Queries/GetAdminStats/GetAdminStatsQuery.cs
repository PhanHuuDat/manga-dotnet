using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Admin.Queries.GetAdminStats;

/// <summary>
/// Returns platform-wide aggregate statistics. Requires AdminViewStats permission.
/// </summary>
[RequirePermission(nameof(Permission.AdminViewStats))]
public record GetAdminStatsQuery : IRequest<Result<AdminStatsDto>>;
