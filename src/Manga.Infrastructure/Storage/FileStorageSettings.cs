namespace Manga.Infrastructure.Storage;

/// <summary>
/// Configuration for local file storage.
/// </summary>
public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    /// <summary>Base directory for file storage (relative or absolute).</summary>
    public string BasePath { get; set; } = "uploads";

    /// <summary>Base URL prefix for accessing stored files.</summary>
    public string BaseUrl { get; set; } = "/api/attachments";
}
