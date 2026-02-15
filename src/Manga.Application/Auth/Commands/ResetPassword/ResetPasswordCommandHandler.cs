using Manga.Application.Common.Helpers;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var hashedToken = TokenHasher.Hash(request.Token);

        var token = await db.VerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == hashedToken &&
                t.UserId == request.UserId &&
                t.TokenType == VerificationTokenType.PasswordReset, ct);

        if (token is null || !token.IsValid)
            return Result.Failure("Invalid or expired reset link.");

        token.UsedAt = DateTimeOffset.UtcNow;
        token.User.PasswordHash = passwordHasher.Hash(request.NewPassword);

        // Revoke all refresh tokens to force re-login everywhere
        var refreshTokens = await db.RefreshTokens
            .Where(t => t.UserId == request.UserId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var rt in refreshTokens)
            rt.RevokedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
