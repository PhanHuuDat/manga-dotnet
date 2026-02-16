using Manga.Application.Attachments.DTOs;
using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Attachments.Commands.UploadAttachment;

/// <summary>
/// Upload a file and create an Attachment record.
/// The endpoint extracts multipart data and passes it as a stream.
/// </summary>
[RequirePermission(nameof(Permission.AttachmentUpload))]
public record UploadAttachmentCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize,
    AttachmentType Type) : IRequest<Result<AttachmentDto>>;
