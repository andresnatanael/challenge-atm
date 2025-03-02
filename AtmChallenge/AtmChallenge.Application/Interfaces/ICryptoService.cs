namespace AtmChallenge.Application.Interfaces;

public interface ICryptoService
{
    string EncryptData(string content);
    string DecryptData(string encryptedContent);
}