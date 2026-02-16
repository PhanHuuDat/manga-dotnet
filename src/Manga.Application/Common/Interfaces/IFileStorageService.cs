namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Abstraction for file storage operations (local, S3, etc.).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Store a file and return the storage path, public URL, and file size.
    /// </summary>
    Task<FileStorageResult> StoreAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string subfolder,
        CancellationToken ct = default);

    /// <summary>
    /// Delete a stored file by its storage path.
    /// </summary>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Get a readable stream for a stored file. Returns null if not found.
    /// </summary>
    Task<Stream?> GetAsync(string storagePath, CancellationToken ct = default);
}

/// <summary>
/// Result of storing a file, containing the storage path and public URL.
/// </summary>
public record FileStorageResult(string StoragePath, string Url, long FileSize);
