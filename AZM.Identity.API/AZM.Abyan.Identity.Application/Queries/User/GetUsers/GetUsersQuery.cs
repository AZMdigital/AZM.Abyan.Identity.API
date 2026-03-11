using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.User.GetUsers;

public class GetUsersQuery : IRequest<Result<List<UserResponse>>>
{
}
