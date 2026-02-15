using Manga.Application.Common.Helpers;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(IAppDbContext db) : IRequestHandler<VerifyEmailCommand, Result>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var hashedToken = TokenHasher.Hash(request.Token);

        var token = await db.VerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == hashedToken &&
                t.UserId == request.UserId &&
                t.TokenType == VerificationTokenType.EmailVerification, ct);

        if (token is null || !token.IsValid)
            return Result.Failure("Invalid or expired verification link.");

        token.UsedAt = DateTimeOffset.UtcNow;
        token.User.EmailConfirmed = true;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
