using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Users.Commands.UploadAvatar;

[Authorize]
public record UploadAvatarCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize) : IRequest<Result<string>>;
