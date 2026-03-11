using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.User.GetUsers;

public class GetUsersQueryHandler(IUserService userService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetUsersQuery, Result<List<UserResponse>>>
{
    private readonly IUserService _userService = userService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<List<UserResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(cancellationToken);
        return Result<List<UserResponse>>.Success(users, _localizer["OperationSuccess"]);
    }
}
