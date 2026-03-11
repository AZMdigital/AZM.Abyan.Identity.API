using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Auth.GetCurrentUserInfo;

public class GetCurrentUserInfoQuery : IRequest<Result<UserInfoResponse>>
{
    public string AccessToken { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Username { get; set; }
}
