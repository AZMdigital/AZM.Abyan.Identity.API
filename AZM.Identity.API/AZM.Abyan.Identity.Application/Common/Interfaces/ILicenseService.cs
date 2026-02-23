using AZM.Abyan.Identity.Application.DTOs;

namespace AZM.Abyan.Identity.Application.Common.Interfaces;

public interface ILicenseService
{
    LicenseFileDto? Parse(string rawJson);
    bool   ValidateSignature(string rawJson, string publicKeyPem);
    string Canonicalize(string rawJson);
    string ComputeHash(string rawJson);
    bool   VerifyHash(string rawJson, string storedHash);
    string Sign(string rawJson, string privateKeyPem);
}
