using FluentValidation;
using Manga.Domain.Enums;

namespace Manga.Application.Admin.Commands.UpdateUserRole;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.Role)
            .Must(r => Enum.IsDefined(typeof(UserRole), r))
            .WithMessage("Invalid role value.");
    }
}
