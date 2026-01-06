namespace AZM.Abyan.Identity.Domain.Interfaces;

public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
    string? GetCurrentUserEmail();
    string? GetCurrentUserName();
    bool IsAuthenticated();
    bool IsInRole(string role);
}
