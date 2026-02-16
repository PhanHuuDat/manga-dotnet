using Manga.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Manga.Infrastructure.Storage;

/// <summary>
/// Stores files on the local filesystem. Suitable for development.
/// </summary>
public class LocalFileStorageService(IOptions<FileStorageSettings> options) : IFileStorageService
{
    private readonly FileStorageSettings _settings = options.Value;

    public async Task<FileStorageResult> StoreAsync(
        Stream fileStream, string fileName, string contentType,
        string subfolder, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid()}{extension}";
        var relativePath = Path.Combine(subfolder, storedName);
        var fullPath = Path.GetFullPath(Path.Combine(_settings.BasePath, relativePath));
        if (!fullPath.StartsWith(Path.GetFullPath(_settings.BasePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");

        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fs, ct);
        var fileSize = fs.Length;

        var url = $"{_settings.BaseUrl}/{relativePath.Replace('\\', '/')}";
        return new FileStorageResult(relativePath, url, fileSize);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_settings.BasePath, storagePath));
        if (!fullPath.StartsWith(Path.GetFullPath(_settings.BasePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<Stream?> GetAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_settings.BasePath, storagePath));
        if (!fullPath.StartsWith(Path.GetFullPath(_settings.BasePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }
}
