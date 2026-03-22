using Manga.Application.Common.Models;
using Manga.Application.Common.Services;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;

namespace Manga.Application.Tests.Services;

public class AttachmentValidationServiceTests
{
    [Fact]
    public async Task ValidateExistsAsync_WithNullAttachmentId_ReturnsSuccess()
    {
        using var db = TestDbContextFactory.Create();
        var service = new AttachmentValidationService(db);

        var result = await service.ValidateExistsAsync(null, "Cover", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateExistsAsync_WithExistingAttachment_ReturnsSuccess()
    {
        using var db = TestDbContextFactory.Create();
        var attachment = new Attachment
        {
            FileName = "cover.jpg",
            StoragePath = "/covers",
            Url = "https://example.com/cover.jpg",
            ContentType = "image/jpeg"
        };
        db.Attachments.Add(attachment);
        await db.SaveChangesAsync();

        var service = new AttachmentValidationService(db);
        var result = await service.ValidateExistsAsync(attachment.Id, "Cover", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateExistsAsync_WithNonexistentAttachment_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var service = new AttachmentValidationService(db);

        var result = await service.ValidateExistsAsync(Guid.NewGuid(), "Cover", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains("Cover attachment not found", result.Errors[0]);
    }

    [Fact]
    public async Task ValidateExistsAsync_UsesFieldNameInErrorMessage()
    {
        using var db = TestDbContextFactory.Create();
        var service = new AttachmentValidationService(db);

        var result = await service.ValidateExistsAsync(Guid.NewGuid(), "Banner", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains("Banner", result.Errors[0]);
    }

    [Fact]
    public async Task ValidateExistsAsync_WithMultipleAttachments_OnlyValidatesRequested()
    {
        using var db = TestDbContextFactory.Create();
        var cover = new Attachment
        {
            FileName = "cover.jpg",
            StoragePath = "/covers",
            Url = "https://example.com/cover.jpg",
            ContentType = "image/jpeg"
        };
        var banner = new Attachment
        {
            FileName = "banner.jpg",
            StoragePath = "/banners",
            Url = "https://example.com/banner.jpg",
            ContentType = "image/jpeg"
        };
        db.Attachments.AddRange(cover, banner);
        await db.SaveChangesAsync();

        var service = new AttachmentValidationService(db);
        var result = await service.ValidateExistsAsync(cover.Id, "Cover", CancellationToken.None);

        Assert.Null(result);
    }
}
