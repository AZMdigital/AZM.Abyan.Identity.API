using System.Security.Cryptography;

namespace AZM.Abyan.Identity.Application.Common.Interfaces;

public interface IRsaKeyProvider
{
    string GetPublicKeyPem();
    string GetPrivateKeyPem();
    RSA GetRsa(); // Returns configured RSA instance for direct use
}
