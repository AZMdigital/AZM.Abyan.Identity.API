using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.User.ResetPassword;

public class ResetUserPasswordCommandHandler(IUserService userService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<ResetUserPasswordCommand, Result<bool>>
{
    private readonly IUserService _userService = userService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        await _userService.ResetUserPasswordAsync(request.UserId, request.Request, cancellationToken);
        return Result<bool>.Success(true, _localizer["OperationSuccess"]);
    }
}
