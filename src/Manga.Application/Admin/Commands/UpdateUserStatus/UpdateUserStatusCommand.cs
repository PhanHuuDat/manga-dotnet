using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Admin.Commands.UpdateUserStatus;

/// <summary>
/// Activates or deactivates a user account. Requires UserUpdate permission.
/// </summary>
[RequirePermission(nameof(Permission.UserUpdate))]
public record UpdateUserStatusCommand(Guid UserId, bool IsActive) : IRequest<Result>;
