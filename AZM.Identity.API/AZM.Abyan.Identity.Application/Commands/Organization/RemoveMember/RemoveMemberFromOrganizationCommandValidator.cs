using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Organization.RemoveMember;

public class RemoveMemberFromOrganizationCommandValidator : AbstractValidator<RemoveMemberFromOrganizationCommand>
{
    public RemoveMemberFromOrganizationCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.OrganizationId).NotEmpty().WithMessage(localizer["OrganizationIdRequired"]);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["UserIdRequired"]);
    }
}
