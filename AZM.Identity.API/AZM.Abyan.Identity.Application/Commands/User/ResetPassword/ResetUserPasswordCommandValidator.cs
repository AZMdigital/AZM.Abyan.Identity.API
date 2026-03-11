using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.User.ResetPassword;

public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(localizer["UserIdRequired"])
            .Must(id => Guid.TryParse(id, out _)).WithMessage(localizer["InvalidUserId"]);

        RuleFor(x => x.Request.NewPassword)
            .NotEmpty().WithMessage(localizer["NewPasswordRequired"])
            .MinimumLength(6).WithMessage(localizer["PasswordTooShort"]);
    }
}
