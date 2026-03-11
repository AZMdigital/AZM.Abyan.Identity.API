using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Auth.GetCurrentUserInfo;

public class GetCurrentUserInfoQueryHandler : IRequestHandler<GetCurrentUserInfoQuery, Result<UserInfoResponse>>
{
    private readonly IUserService _userService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetCurrentUserInfoQueryHandler(IUserService userService, IStringLocalizer<SharedResource> localizer)
    {
        _userService = userService;
        _localizer = localizer;
    }

    public async Task<Result<UserInfoResponse>> Handle(GetCurrentUserInfoQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            var user = await _userService.GetUserByUsernameAsync(request.Username!, cancellationToken);
            if (user == null)
                return Result<UserInfoResponse>.NotFound(_localizer["UserNotFound"]);

            userId = user.Id;
        }

        var userInfo = await _userService.GetCurrentUserInfoAsync(userId, request.AccessToken, cancellationToken);

        if (userInfo == null)
            return Result<UserInfoResponse>.NotFound(_localizer["UserNotFound"]);

        return Result<UserInfoResponse>.Success(userInfo);
    }
}
