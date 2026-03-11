using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Client.Update;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.UpdateClientRequest).NotNull().WithMessage(localizer["UpdateClientRequestRequired"]);
        RuleFor(x => x.UpdateClientRequest.ClientId).NotEmpty().WithMessage(localizer["ClientIdRequired"]);
        RuleFor(x => x.UpdateClientRequest.Name).NotEmpty().WithMessage(localizer["ClientNameRequired"]);
    }
}
