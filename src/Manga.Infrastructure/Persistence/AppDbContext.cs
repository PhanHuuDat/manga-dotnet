using Manga.Application.Common.Interfaces;
using Manga.Domain.Common;
using Manga.Domain.Entities;
using Manga.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Manga.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<MangaSeries> MangaSeries => Set<MangaSeries>();
    public DbSet<AlternativeTitle> AlternativeTitles => Set<AlternativeTitle>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MangaGenre> MangaGenres => Set<MangaGenre>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<ChapterPage> ChapterPages => Set<ChapterPage>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<ReadingHistory> ReadingHistories => Set<ReadingHistory>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<ViewStat> ViewStats => Set<ViewStat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global soft-delete query filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter("SoftDelete",
                        QueryFilterExpressionHelper.BuildSoftDeleteFilter(entityType.ClrType));
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// Helper to build soft-delete lambda expressions dynamically.
/// </summary>
internal static class QueryFilterExpressionHelper
{
    public static System.Linq.Expressions.LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
        var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(condition, parameter);
    }
}
