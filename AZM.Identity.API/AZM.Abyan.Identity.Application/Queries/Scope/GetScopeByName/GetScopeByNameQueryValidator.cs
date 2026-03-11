using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Scope.GetScopeByName;

public class GetScopeByNameQueryValidator : AbstractValidator<GetScopeByNameQuery>
{
    public GetScopeByNameQueryValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientId).NotEmpty().WithMessage(localizer["ClientIdRequired"]);
        RuleFor(x => x.ScopeName).NotEmpty().WithMessage(localizer["ScopeNameRequired"]);
    }
}
