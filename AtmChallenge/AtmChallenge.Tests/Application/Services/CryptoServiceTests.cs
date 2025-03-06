namespace AtmChallenge.Tests.Application.Services;

using System;
using Xunit;

public class CryptoServiceTests
{

    private const string ValidKeyBase64 = "qHcLXZMR29eBLlCPXnD2cjXKMxaBjRyz8ytSi2BcK+Y=";
    private const string ValidIVBase64 = "ka0v0cLgGIrh76Ve/P+wuA==";

    [Fact]
    public void Constructor_ThrowsException_WhenAesSecretKeyNotSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AES_SECRET_KEY", null);
        Environment.SetEnvironmentVariable("AES_IV", ValidIVBase64);

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => new CryptoService());
        Assert.Contains("AES_SECRET_KEY environment variable is not set", ex.Message);
    }

    [Fact]
    public void Constructor_ThrowsException_WhenAesIvNotSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AES_SECRET_KEY", ValidKeyBase64);
        Environment.SetEnvironmentVariable("AES_IV", null);

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => new CryptoService());
        Assert.Contains("AES_IV environment variable is not set", ex.Message);
    }

    [Fact]
    public void EncryptData_DecryptData_ReturnsOriginalText()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AES_SECRET_KEY", ValidKeyBase64);
        Environment.SetEnvironmentVariable("AES_IV", ValidIVBase64);

        var service = new CryptoService();
        var originalText = "Hello, world!";

        // Act
        var encrypted = service.EncryptData(originalText);
        var decrypted = service.DecryptData(encrypted);

        // Assert
        Assert.Equal(originalText, decrypted);
    }
}