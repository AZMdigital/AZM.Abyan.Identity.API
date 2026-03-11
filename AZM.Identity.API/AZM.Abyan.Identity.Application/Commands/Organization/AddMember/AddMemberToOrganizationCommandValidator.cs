using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.Organization.AddMember;

public class AddMemberToOrganizationCommandValidator : AbstractValidator<AddMemberToOrganizationCommand>
{
    public AddMemberToOrganizationCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.OrganizationId).NotEmpty().WithMessage(localizer["OrganizationIdRequired"]);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["UserIdRequired"]);
    }
}
