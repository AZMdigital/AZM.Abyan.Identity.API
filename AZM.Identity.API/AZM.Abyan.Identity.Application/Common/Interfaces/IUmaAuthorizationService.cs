using Microsoft.AspNetCore.Http;

namespace AZM.Abyan.Identity.Application.Common.Interfaces;

public interface IUmaAuthorizationService
{
    Task<bool> IsAuthorizedAsync(
        HttpContext context,
        string accessToken,
        CancellationToken cancellationToken);
}
