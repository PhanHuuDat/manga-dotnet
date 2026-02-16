using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.ReadingHistories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.ReadingHistories.Queries.GetResumePoint;

public class GetResumePointQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetResumePointQuery, Result<ResumePointDto?>>
{
    public async Task<Result<ResumePointDto?>> Handle(
        GetResumePointQuery request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var dto = await db.ReadingHistories
            .Where(rh => rh.UserId == userId && rh.MangaSeriesId == request.MangaSeriesId)
            .Select(rh => new ResumePointDto(
                rh.ChapterId,
                rh.Chapter.Title,
                rh.Chapter.ChapterNumber,
                rh.LastPageNumber))
            .FirstOrDefaultAsync(ct);

        return Result<ResumePointDto?>.Success(dto);
    }
}
