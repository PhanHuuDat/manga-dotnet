using Manga.Application.Auth.DTOs;
using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RawRefreshToken) : IRequest<Result<LoginTokenResult>>;
