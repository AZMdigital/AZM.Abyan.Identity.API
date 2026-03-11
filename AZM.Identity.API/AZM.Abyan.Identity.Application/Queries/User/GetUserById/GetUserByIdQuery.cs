using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.User.GetUserById;

public class GetUserByIdQuery(string userId) : IRequest<Result<UserResponse>>
{
    public string UserId { get; set; } = userId;
}
