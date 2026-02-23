using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.DTOs;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class LicenseService : ILicenseService
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LicenseFileDto? Parse(string rawJson)
    {
        try { return JsonSerializer.Deserialize<LicenseFileDto>(rawJson, _options); }
        catch { return null; }
    }

    public string Canonicalize(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var dict = new Dictionary<string, object?>();

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (!string.Equals(prop.Name, "signature", StringComparison.OrdinalIgnoreCase))
            {
                dict[prop.Name] = GetValue(prop.Value);
            }
        }

        // Ordered dictionary to ensure deterministic JSON representation
        var sorted = new SortedDictionary<string, object?>(dict, StringComparer.Ordinal);
        return JsonSerializer.Serialize(sorted);
    }

    public string ComputeHash(string rawJson)
    {
        var canonical = Canonicalize(rawJson);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyHash(string rawJson, string storedHash)
    {
        return ComputeHash(rawJson) == storedHash;
    }

    public bool ValidateSignature(string rawJson, string publicKeyPem)
    {
        try
        {
            var dto = Parse(rawJson);
            if (string.IsNullOrEmpty(dto?.Signature)) return false;

            var canonical = Canonicalize(rawJson);
            var dataToVerify = Encoding.UTF8.GetBytes(canonical);
            var signatureBytes = Convert.FromBase64String(dto.Signature);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            return rsa.VerifyData(dataToVerify, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    public string Sign(string rawJson, string privateKeyPem)
    {
        var canonical = Canonicalize(rawJson);
        var dataToSign = Encoding.UTF8.GetBytes(canonical);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var signatureBytes = rsa.SignData(dataToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signatureBytes);
    }

    private static object? GetValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(GetValue).ToList(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => GetValue(p.Value)),
            _ => element.GetRawText()
        };
    }
}
