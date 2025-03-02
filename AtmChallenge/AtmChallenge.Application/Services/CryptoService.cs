using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AtmChallenge.Application.Interfaces;

public class CryptoService : ICryptoService
{
    private readonly RSA _rsa;

    public CryptoService()
    {
        _rsa = RSA.Create();

        string publicKeyPath = Environment.GetEnvironmentVariable("PUBLIC_KEY_PATH");
        string privateKeyPath = Environment.GetEnvironmentVariable("PRIVATE_KEY_PATH");

        if (string.IsNullOrEmpty(publicKeyPath) || string.IsNullOrEmpty(privateKeyPath))
        {
            throw new Exception("❌ PUBLIC_KEY_PATH or PRIVATE_KEY_PATH environment variable is not set.");
        }

        LoadKeys(publicKeyPath, privateKeyPath);
    }

    private void LoadKeys(string publicKeyPath, string privateKeyPath)
    {
        try
        {
            if (!File.Exists(publicKeyPath))
            {
                throw new FileNotFoundException($"❌ Public Key Missing: {publicKeyPath}");
            }

            string publicKey = File.ReadAllText(publicKeyPath);
            _rsa.ImportFromPem(publicKey);
            Console.WriteLine($"🔹 RSA Public Key Loaded: {publicKeyPath}");

            if (!File.Exists(privateKeyPath))
            {
                throw new FileNotFoundException($"❌ Private Key Missing: {privateKeyPath}");
            }

            string privateKey = File.ReadAllText(privateKeyPath);
            _rsa.ImportFromPem(privateKey);
            Console.WriteLine($"🔹 RSA Private Key Loaded: {privateKeyPath}");
        }
        catch (Exception ex)
        {
            throw new Exception($"❌ Failed to load RSA Keys: {ex.Message}");
        }
    }

    /// 🔹 **Encrypt any string content**
    public string EncryptData(string content)
    {
        byte[] data = Encoding.UTF8.GetBytes(content);
        byte[] encryptedData = _rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encryptedData);
    }

    /// 🔹 **Decrypt any encrypted content**
    public string DecryptData(string encryptedContent)
    {
        byte[] encryptedData = Convert.FromBase64String(encryptedContent);
        byte[] decryptedData = _rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decryptedData);
    }
}