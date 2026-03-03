using AZM.Abyan.Identity.Domain.Entities;

namespace AZM.Abyan.Identity.Application.Common.Interfaces;

public interface IJwtIssuerService
{
    string IssueToken(License license, Client client);
    string IssueToken(Guid licenseId, DateTime expiresAt);
    string IssueRefreshToken(License license, Client client);
    string GenerateRefreshToken();
}
