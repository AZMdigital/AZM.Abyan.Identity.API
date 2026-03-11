using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Policy.GetPolicies;

public class GetPoliciesQueryValidator : AbstractValidator<GetPoliciesQuery>
{
    public GetPoliciesQueryValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientId).NotEmpty().WithMessage(localizer["ClientIdRequired"]);
    }
}
