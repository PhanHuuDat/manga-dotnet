using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Admin.Commands.UpdateUserStatus;

/// <summary>
/// Sets IsActive flag on a user account. Prevents an admin from deactivating themselves.
/// </summary>
public class UpdateUserStatusCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateUserStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateUserStatusCommand request, CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UserId, out var callerId))
            return Result.Failure("User not authenticated.");

        if (request.UserId == callerId)
            return Result.Failure("Cannot change your own account status.");

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null)
            return Result.Failure("User not found.");

        user.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
