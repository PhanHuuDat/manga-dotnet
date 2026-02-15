using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IAppDbContext db,
    ITokenBlacklistService blacklist,
    ICurrentUserService currentUser) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var remaining = request.AccessTokenExpiry - DateTimeOffset.UtcNow;
        if (remaining > TimeSpan.Zero)
            await blacklist.BlacklistAsync(request.Jti, remaining, ct);

        if (Guid.TryParse(currentUser.UserId, out var userId))
        {
            var activeTokens = await db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var token in activeTokens)
                token.RevokedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
