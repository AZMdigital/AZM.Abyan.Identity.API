using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Client.Delete;

public class DeleteClientCommandValidator : AbstractValidator<DeleteClientCommand>
{
    public DeleteClientCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.RealmName).NotEmpty().WithMessage(localizer["RealmNameRequired"]);
        RuleFor(x => x.ClientId).NotEmpty().WithMessage(localizer["ClientIdRequired"]);
    }
}
