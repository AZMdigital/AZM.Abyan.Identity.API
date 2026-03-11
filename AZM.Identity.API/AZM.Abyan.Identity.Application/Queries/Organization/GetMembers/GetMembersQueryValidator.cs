using FluentValidation;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetMembers;

public class GetMembersQueryValidator : AbstractValidator<GetMembersQuery>
{
    public GetMembersQueryValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.OrganizationId).NotEmpty().WithMessage(localizer["OrganizationIdRequired"]);
    }
}
