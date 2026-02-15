namespace Manga.Domain.Interfaces;

/// <summary>
/// Unit of work pattern to coordinate repository transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
