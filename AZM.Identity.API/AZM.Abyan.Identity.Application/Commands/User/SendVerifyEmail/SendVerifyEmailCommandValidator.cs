using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.User.SendVerifyEmail;

public class SendVerifyEmailCommandValidator : AbstractValidator<SendVerifyEmailCommand>
{
    public SendVerifyEmailCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(localizer["UserIdRequired"]);
    }
}
