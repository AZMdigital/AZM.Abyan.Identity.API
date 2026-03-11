using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClientByName;

public class GetClientByNameQueryValidator : AbstractValidator<GetClientByNameQuery>
{
    public GetClientByNameQueryValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientName).NotEmpty().WithMessage(localizer["ClientNameRequired"]);
    }
}
