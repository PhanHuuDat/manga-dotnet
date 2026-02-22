using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Admin.Commands.UpdateUserRole;

/// <summary>
/// Grants or revokes a role for a user. Requires UserManageRoles permission.
/// </summary>
[RequirePermission(nameof(Permission.UserManageRoles))]
public record UpdateUserRoleCommand(Guid UserId, UserRole Role, bool Grant) : IRequest<Result>;
