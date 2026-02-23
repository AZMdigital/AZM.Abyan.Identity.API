using FluentValidation;

namespace AZM.Abyan.Identity.Application.Commands.License.ActivateLicense;

public class ActivateLicenseValidator : AbstractValidator<ActivateLicenseCommand>
{
    public ActivateLicenseValidator()
    {
        RuleFor(x => x.LicenseFile)
            .NotEmpty().WithMessage("License file is required.")
            .Must(BeValidJson).WithMessage("License file must be valid JSON.");
    }

    private static bool BeValidJson(string raw)
    {
        try { System.Text.Json.JsonDocument.Parse(raw); return true; }
        catch { return false; }
    }
}
