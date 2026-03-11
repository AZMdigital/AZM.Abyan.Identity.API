using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;
using AZM.Abyan.Identity.Application.Resources;

namespace AZM.Abyan.Identity.Application.Commands.Auth.Logout;

public class LogoutCommandHandler(IAuthService authService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IAuthService _authService = authService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _authService.LogoutUserAsync(request.UserId.ToString(), cancellationToken);
        return Result<bool>.Success(true, _localizer["OperationSuccess"]);
    }
}
