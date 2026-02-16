using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Attachments.Queries.GetAttachmentFile;

public record GetAttachmentFileQuery(Guid Id) : IRequest<Result<AttachmentFileResult>>;

public record AttachmentFileResult(Stream FileStream, string ContentType, string FileName);
