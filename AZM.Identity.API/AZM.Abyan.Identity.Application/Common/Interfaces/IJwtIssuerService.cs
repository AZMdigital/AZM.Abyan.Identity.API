using AZM.Abyan.Identity.Domain.Entities;

namespace AZM.Abyan.Identity.Application.Common.Interfaces;

public interface IJwtIssuerService
{
    string IssueToken(License license, List<Client> clients);
    //string IssueToken(Guid licenseId, DateTime expiresAt);
    string IssueRefreshToken(License license, List<Client> clients);
    string GenerateRefreshToken();
}
