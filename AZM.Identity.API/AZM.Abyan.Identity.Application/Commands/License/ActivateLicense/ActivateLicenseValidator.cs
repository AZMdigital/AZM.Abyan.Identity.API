using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.License.ActivateLicense;

public class ActivateLicenseValidator : AbstractValidator<ActivateLicenseCommand>
{
    public ActivateLicenseValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.LicenseFile)
            .NotEmpty().WithMessage(localizer["LicenseFileRequired"])
            .Must(BeValidJson).WithMessage(localizer["LicenseFileInvalidJson"]);
    }

    private static bool BeValidJson(string raw)
    {
        try { System.Text.Json.JsonDocument.Parse(raw); return true; }
        catch { return false; }
    }
}
