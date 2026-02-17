using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AZM.Abyan.Identity.Application.Services;

public interface ITenantProvider
{
    string GetTenant(ClaimsPrincipal user);
}
