using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Commands.DeleteManga;

public class DeleteMangaCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteMangaCommand, Result>
{
    public async Task<Result> Handle(DeleteMangaCommand request, CancellationToken ct)
    {
        var manga = await db.MangaSeries
            .FirstOrDefaultAsync(m => m.Id == request.Id, ct);

        if (manga is null)
            return Result.Failure("Manga not found.");

        // Soft delete — AuditableEntityInterceptor sets IsDeleted + DeletedAt
        manga.IsDeleted = true;
        manga.DeletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
