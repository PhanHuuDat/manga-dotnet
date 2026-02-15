using Manga.Application.Common.Helpers;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IAppDbContext db,
    ITokenService tokenService,
    IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        // Always return success to prevent user enumeration
        if (user is null)
            return Result.Success();

        var rawToken = tokenService.GenerateEmailToken();
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.PasswordReset,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(3),
        });

        await db.SaveChangesAsync(ct);
        await emailService.SendPasswordResetAsync(user.Email, user.Username, rawToken, user.Id, ct);

        return Result.Success();
    }
}
