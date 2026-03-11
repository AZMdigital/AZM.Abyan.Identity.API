using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Resource.GetResourceById;

public class GetResourceByIdQueryValidator : AbstractValidator<GetResourceByIdQuery>
{
    public GetResourceByIdQueryValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientId).NotEmpty().WithMessage(localizer["ClientIdRequired"]);
        RuleFor(x => x.ResourceId).NotEmpty().WithMessage(localizer["ResourceIdRequired"]);
    }
}
