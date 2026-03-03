using System.Security.Cryptography;

var rsa = RSA.Create(2048);
var privateKey = rsa.ExportPkcs8PrivateKeyPem();
var publicKey = rsa.ExportSubjectPublicKeyInfoPem();

Console.WriteLine("--- PRIVATE KEY ---");
Console.WriteLine(privateKey);
Console.WriteLine("\n--- PUBLIC KEY ---");
Console.WriteLine(publicKey);
