using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.ProtocolMapper.Disable;

public class DisableProtocolMapperCommandValidator : AbstractValidator<DisableProtocolMapperCommand>
{
    public DisableProtocolMapperCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientScopeId).NotEmpty().WithMessage(localizer["ClientScopeIdRequired"]);
        RuleFor(x => x.MapperId).NotEmpty().WithMessage(localizer["MapperIdRequired"]);
    }
}
