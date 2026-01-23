using System.Security.Cryptography;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using FluentAssertions;
using Xunit;

namespace CookieCommerce.CustomerDomain.Tests.InfrastructureTests;

public class EncryptionServiceTests
{
    [Fact]
    public void EncryptDecrypt_Roundtrip_Works()
    {
        var svc = new EncryptionService();
        var key = RandomNumberGenerator.GetBytes(32);
        var plaintext = "The quick brown fox jumps over the lazy dog";

        var cipher = svc.Encrypt(plaintext, key);
        cipher.Should().NotBeNullOrWhiteSpace();

        var decrypted = svc.Decrypt(cipher, key);
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_InvalidKeyLength_Throws()
    {
        var svc = new EncryptionService();
        var key = RandomNumberGenerator.GetBytes(10); // invalid length

        Action act = () => svc.Encrypt("hello", key);

        act.Should().Throw<ArgumentException>().WithMessage("Invalid key length*");
    }

    [Fact]
    public void Decrypt_InvalidBase64_ThrowsFormatException()
    {
        var svc = new EncryptionService();
        var key = RandomNumberGenerator.GetBytes(16);

        Action act = () => svc.Decrypt("not-base64!!", key);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        var svc = new EncryptionService();
        var key = RandomNumberGenerator.GetBytes(16);
        var plaintext = "sensitive data";

        var cipher = svc.Encrypt(plaintext, key);
        var bytes = Convert.FromBase64String(cipher);

        // Flip a byte in the ciphertext region (after nonce+tag)
        if (bytes.Length > 28)
            bytes[28] ^= 0xFF;

        var tampered = Convert.ToBase64String(bytes);

        Action act = () => svc.Decrypt(tampered, key);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Encrypt_NullArguments_Throw()
    {
        var svc = new EncryptionService();
        var key = RandomNumberGenerator.GetBytes(16);

        Action a1 = () => svc.Encrypt(null!, key);
        Action a2 = () => svc.Encrypt("hello", null!);

        a1.Should().Throw<ArgumentNullException>();
        a2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Decrypt_NullArguments_Throw()
    {
        var svc = new EncryptionService();
        var key = RandomNumberGenerator.GetBytes(16);

        Action a1 = () => svc.Decrypt(null!, key);
        Action a2 = () => svc.Decrypt("abc", null!);

        a1.Should().Throw<ArgumentNullException>();
        a2.Should().Throw<ArgumentNullException>();
    }
}
