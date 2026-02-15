using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword) : IRequest<Result>;
