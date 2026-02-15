using Manga.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<MangaSeries> MangaSeries { get; }
    DbSet<AlternativeTitle> AlternativeTitles { get; }
    DbSet<Genre> Genres { get; }
    DbSet<MangaGenre> MangaGenres { get; }
    DbSet<Chapter> Chapters { get; }
    DbSet<ChapterPage> ChapterPages { get; }
    DbSet<Comment> Comments { get; }
    DbSet<CommentReaction> CommentReactions { get; }
    DbSet<Bookmark> Bookmarks { get; }
    DbSet<ReadingHistory> ReadingHistories { get; }
    DbSet<Person> Persons { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<ViewStat> ViewStats { get; }
    DbSet<UserRoleMapping> UserRoleMappings { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<VerificationToken> VerificationTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
