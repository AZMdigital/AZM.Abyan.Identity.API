using AZM.Abyan.Identity.Application.DTOs.Users;

namespace AZM.Abyan.Identity.Application.Services;

public interface IUserService
{
    Task<string> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task EnableUserAsync(string userId, bool enabled, CancellationToken cancellationToken = default);
}

