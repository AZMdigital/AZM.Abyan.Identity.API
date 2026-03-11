using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.User.ForgotPassword;

public class ForgotPasswordCommandHandler(IUserService userService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IUserService _userService = userService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var sent = await _userService.ForgotPasswordAsync(request.Request, cancellationToken);

        if (!sent)
        {
            return Result<bool>.NotFound(_localizer["UserNotFound"]);
        }

        return Result<bool>.Success(true, _localizer["OperationSuccess"]);
    }
}
