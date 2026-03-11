using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.Auth.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(localizer["RefreshTokenRequired"]);
    }
}
