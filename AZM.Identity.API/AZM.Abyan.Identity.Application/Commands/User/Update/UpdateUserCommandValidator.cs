using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.User.Update;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UpdateUserRequest.UserId)
            .NotEmpty().WithMessage(localizer["UserIdRequired"])
            .Must(id => Guid.TryParse(id, out _)).WithMessage(localizer["InvalidUserId"]);

        RuleFor(x => x.UpdateUserRequest.Email)
            .NotEmpty().WithMessage(localizer["EmailRequired"])
            .EmailAddress().WithMessage(localizer["InvalidEmailFormat"]);
        RuleFor(x => x.UpdateUserRequest.FirstName)
            .NotEmpty().WithMessage(localizer["FirstNameRequired"]);

        RuleFor(x => x.UpdateUserRequest.LastName)
            .NotEmpty().WithMessage(localizer["LastNameRequired"]);
    }
}
