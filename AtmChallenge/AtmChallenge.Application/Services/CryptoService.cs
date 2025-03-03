using System.Security.Cryptography;
using System.Text;
using AtmChallenge.Application.Interfaces;

public class CryptoService : ICryptoService
{
    private readonly byte[] _aesKey;
    private readonly byte[] _aesIV;

    public CryptoService()
    {
        string keyEnv = Environment.GetEnvironmentVariable("AES_SECRET_KEY");
        string ivEnv = Environment.GetEnvironmentVariable("AES_IV");

        if (string.IsNullOrEmpty(keyEnv))
        {
            throw new Exception("❌ AES_SECRET_KEY environment variable is not set.");
        }

        if (string.IsNullOrEmpty(ivEnv))
        {
            throw new Exception("❌ AES_IV environment variable is not set.");
        }

        _aesKey = Convert.FromBase64String(keyEnv);
        _aesIV = Convert.FromBase64String(ivEnv);

        if (_aesIV.Length != 16)
        {
            throw new Exception("❌ AES_IV must be a 16-byte (128-bit) Base64 encoded value.");
        }
    }
    
    public string EncryptData(string content)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = _aesKey;
            aes.IV = _aesIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(content);
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    /// 🔹 **Decrypt AES-encrypted data**
    public string DecryptData(string encryptedContent)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = _aesKey;
            aes.IV = _aesIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] cipherBytes = Convert.FromBase64String(encryptedContent);
            using (MemoryStream ms = new MemoryStream(cipherBytes))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}