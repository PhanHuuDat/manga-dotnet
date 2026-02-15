using Manga.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Tests.Auth;

/// <summary>
/// Creates InMemory AppDbContext instances for unit tests.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
