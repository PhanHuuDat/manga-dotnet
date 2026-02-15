using Manga.Application.Auth.DTOs;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetCurrentUserQuery, Result<UserProfileResponse>>
{
    public async Task<Result<UserProfileResponse>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
            return Result<UserProfileResponse>.Failure("User not authenticated.");

        var user = await db.Users
            .Include(u => u.UserRoles)
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Result<UserProfileResponse>.Failure("User not found.");

        var roles = user.UserRoles.Select(r => r.Role.ToString()).ToList();

        return Result<UserProfileResponse>.Success(new UserProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.DisplayName,
            user.Avatar?.Id.ToString(),
            user.Level,
            user.EmailConfirmed,
            roles));
    }
}
