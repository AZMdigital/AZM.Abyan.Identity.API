using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.User.Update;

public class UpdateUserCommand(UpdateUserRequest updateUserRequest) : IRequest<Result<bool>>
{
    public UpdateUserRequest UpdateUserRequest { get; } = updateUserRequest;
}
