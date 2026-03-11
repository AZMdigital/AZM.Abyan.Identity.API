using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class JwtIssuerService(IRsaKeyProvider rsaKeyProvider) : IJwtIssuerService
{
    public string IssueToken(License license, List<Client> clients)
    {
        var key = new RsaSecurityKey(rsaKeyProvider.GetRsa());
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var clientIds = clients.Select(c => c.Id.ToString()).ToList();
        var clientNames = clients.Select(c => c.Name ?? "").ToList();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("token_type", "access"),
            new Claim("license_id", license.Id.ToString()),
            new Claim("tenant_id", license.TenantId.ToString()),
            new Claim("client_ids",
            JsonSerializer.Serialize(clientIds),
            JsonClaimValueTypes.Json),
            new Claim("client_names",
            JsonSerializer.Serialize(clientNames),
            JsonClaimValueTypes.Json),
            new Claim("package", license.PackageName ?? "")
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
    public string IssueRefreshToken(License license, List<Client> clients)
    {
        var key = new RsaSecurityKey(rsaKeyProvider.GetRsa());
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var clientIds = clients.Select(c => c.Id.ToString()).ToList();
        var clientNames = clients.Select(c => c.Name ?? "").ToList();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("token_type", "refresh"),
            new Claim("license_id", license.Id.ToString()),
            new Claim("client_ids",
            JsonSerializer.Serialize(clientIds),
            JsonClaimValueTypes.Json),
            new Claim("client_names",
            JsonSerializer.Serialize(clientNames),
            JsonClaimValueTypes.Json),
            new Claim("package", license.PackageName ?? "")
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    //public string IssueToken(Guid licenseId, DateTime expiresAt)
    //{
    //    var key = new RsaSecurityKey(rsaKeyProvider.GetRsa());
    //    var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

    //    var claims = new[]
    //    {
    //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //        new Claim("token_type", "access"),
    //        new Claim("license_id", licenseId.ToString())
    //    };

    //    var descriptor = new SecurityTokenDescriptor
    //    {
    //        Subject = new ClaimsIdentity(claims),
    //        Expires = expiresAt,
    //        SigningCredentials = credentials
    //    };

    //    var handler = new JwtSecurityTokenHandler();
    //    return handler.WriteToken(handler.CreateToken(descriptor));
    //}
}
