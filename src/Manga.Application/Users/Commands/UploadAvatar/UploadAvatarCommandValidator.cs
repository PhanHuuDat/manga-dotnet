using FluentValidation;

namespace Manga.Application.Users.Commands.UploadAvatar;

public class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB for avatars

    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Only JPEG, PNG, and WebP images are allowed for avatars.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("Avatar file size must not exceed 2MB.");
    }
}
