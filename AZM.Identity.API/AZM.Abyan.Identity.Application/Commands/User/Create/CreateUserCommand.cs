using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.User.Create;

public class CreateUserCommand : BaseEntity, IRequest<Result<Guid>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public string? OrganizationName { get; set; } // Optional: if provided, TenantId will be resolved from this
}

