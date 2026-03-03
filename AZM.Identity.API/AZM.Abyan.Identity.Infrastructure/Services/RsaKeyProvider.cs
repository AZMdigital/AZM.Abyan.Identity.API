using System;
using System.Security.Cryptography;
using AZM.Abyan.Identity.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class RsaKeyProvider(IConfiguration config) : IRsaKeyProvider
{
    private RSA? _rsa;
    private readonly object _lock = new();

    public string GetPrivateKeyPem() => Resolve("Licensing:PrivateKeyPem", "LICENSING_PRIVATE_KEY_PEM");
    public string GetPublicKeyPem()  => Resolve("Licensing:PublicKeyPem",  "LICENSING_PUBLIC_KEY_PEM");

    public RSA GetRsa()
    {
        if (_rsa is not null) return _rsa;
        lock (_lock)
        {
            if (_rsa is not null) return _rsa;
            _rsa = RSA.Create();
            _rsa.ImportFromPem(GetPrivateKeyPem());
            return _rsa;
        }
    }

    private string Resolve(string configKey, string envKey)
    {
        var pem = config[configKey] ?? Environment.GetEnvironmentVariable(envKey)
            ?? throw new InvalidOperationException($"RSA key not configured ({configKey}).");
        return pem.Replace("\\n", "\n");
    }
}
