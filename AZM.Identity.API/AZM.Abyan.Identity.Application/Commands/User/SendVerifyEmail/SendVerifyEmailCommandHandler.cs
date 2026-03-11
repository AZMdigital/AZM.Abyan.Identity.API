using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.User.SendVerifyEmail;

public class SendVerifyEmailCommandHandler(IUserService userService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<SendVerifyEmailCommand, Result<bool>>
{
    private readonly IUserService _userService = userService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(SendVerifyEmailCommand request, CancellationToken cancellationToken)
    {
        await _userService.SendVerifyEmailAsync(request.UserId, cancellationToken);
        return Result<bool>.Success(true, _localizer["OperationSuccess"]);
    }
}
