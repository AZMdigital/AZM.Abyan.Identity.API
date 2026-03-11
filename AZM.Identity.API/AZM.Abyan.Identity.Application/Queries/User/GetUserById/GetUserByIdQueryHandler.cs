using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.User.GetUserById;

public class GetUserByIdQueryHandler(IUserService userService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IUserService _userService = userService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result<UserResponse>.NotFound(_localizer["UserNotFound"]);

        return Result<UserResponse>.Success(user, _localizer["OperationSuccess"]);
    }
}
