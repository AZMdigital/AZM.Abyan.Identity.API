using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Create;

public class CreateProtocolMapperCommandValidator : AbstractValidator<CreateProtocolMapperCommand>
{
    public CreateProtocolMapperCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientScopeName).NotEmpty().WithMessage(localizer["ClientScopeNameRequired"]);
        RuleFor(x => x.ClientId).NotEmpty().WithMessage(localizer["ClientIdRequired"]);
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage(localizer["MapperNameRequired"]);
    }
}
