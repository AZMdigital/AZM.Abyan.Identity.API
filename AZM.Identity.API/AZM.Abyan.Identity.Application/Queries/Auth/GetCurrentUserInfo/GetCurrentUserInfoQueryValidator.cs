using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Auth.GetCurrentUserInfo;

public class GetCurrentUserInfoQueryValidator : AbstractValidator<GetCurrentUserInfoQuery>
{
    public GetCurrentUserInfoQueryValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage(localizer["AccessTokenRequired"]);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.UserId) || !string.IsNullOrEmpty(x.Username))
            .WithMessage(localizer["UserIdOrUsernameRequired"]);
    }
}
