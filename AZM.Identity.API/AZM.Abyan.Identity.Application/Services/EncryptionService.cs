using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace AZM.Abyan.Identity.Application.Services;

public class EncryptionService(IConfiguration configuration) : IEncryptionService
{
    private readonly string _key = configuration["EncryptionSettings:Key"]!;

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_key);
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);

        sw.Write(plainText);

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_key);

        var iv = new byte[16];
        Array.Copy(fullCipher, iv, iv.Length);

        aes.IV = iv;

        using var ms = new MemoryStream(fullCipher, 16, fullCipher.Length - 16);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
