using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.Auth.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(localizer["UserIdRequired"]);
    }
}
