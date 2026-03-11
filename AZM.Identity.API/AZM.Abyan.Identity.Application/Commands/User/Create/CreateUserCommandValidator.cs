using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.User.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(localizer["UsernameRequired"]);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["EmailRequired"])
            .EmailAddress().WithMessage(localizer["InvalidEmailFormat"]);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(localizer["FirstNameRequired"]);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(localizer["LastNameRequired"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer["PasswordRequired"])
            .MinimumLength(6).WithMessage(localizer["PasswordTooShort"]);
            
        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage(localizer["OrganizationNameRequired"]);
    }
}
