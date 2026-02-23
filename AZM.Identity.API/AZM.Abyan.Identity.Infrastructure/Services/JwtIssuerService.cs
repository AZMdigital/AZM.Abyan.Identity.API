using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class JwtIssuerService(IRsaKeyProvider rsaKeyProvider) : IJwtIssuerService
{
    public string IssueToken(License license, Client client)
    {
        var key = new RsaSecurityKey(rsaKeyProvider.GetRsa());
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("license_id", license.Id.ToString()),
            new Claim("tenant_id", license.TenantId.ToString()),
            new Claim("client_id", client.Id.ToString()),
            new Claim("client_name", client.Name ?? ""),
            new Claim("package", license.PackageName ?? "")
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
