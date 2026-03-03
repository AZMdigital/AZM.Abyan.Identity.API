using System.Threading;
using System.Threading.Tasks;

namespace AZM.Abyan.Identity.Application.Common.Interfaces;

public interface IKeycloakVerifier
{
    Task<bool> TenantExistsAsync(string realmName, CancellationToken ct = default);
    Task<bool> ClientExistsAsync(string realmName, string clientName, CancellationToken ct = default);
}
