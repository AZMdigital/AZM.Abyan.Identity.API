using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Policy.GetPolicyByName;

public class GetPolicyByNameQueryValidator : AbstractValidator<GetPolicyByNameQuery>
{
    public GetPolicyByNameQueryValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientId).NotEmpty().WithMessage(localizer["ClientIdRequired"]);
        RuleFor(x => x.PolicyName).NotEmpty().WithMessage(localizer["PolicyNameRequired"]);
    }
}
