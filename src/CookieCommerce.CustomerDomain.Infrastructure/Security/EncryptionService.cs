using System.Security.Cryptography;
using CookieCommerce.CustomerDomain.Application.Security;

namespace CookieCommerce.CustomerDomain.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    // Format: base64(nonce (12) || tag (16) || ciphertext)
    public string Encrypt(string plainText, byte[] key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (plainText == null) throw new ArgumentNullException(nameof(plainText));

        // AesGcm requires 16, 24 or 32 byte keys
        if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            throw new ArgumentException("Invalid key length for AES-GCM. Expected 16, 24 or 32 bytes.", nameof(key));

        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

        // 12-byte nonce is recommended for GCM
        var nonce = RandomNumberGenerator.GetBytes(12);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        using (var aes = new AesGcm(key))
        {
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, null);
        }

        var output = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, output, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, output, nonce.Length + tag.Length, ciphertext.Length);

        return Convert.ToBase64String(output);
    }

    public string Decrypt(string ciphertext, byte[] key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));

        var input = Convert.FromBase64String(ciphertext);

        // Expect at least nonce(12) + tag(16)
        if (input.Length < 28) throw new ArgumentException("Invalid ciphertext.", nameof(ciphertext));

        var nonce = new byte[12];
        var tag = new byte[16];
        var actualCipher = new byte[input.Length - nonce.Length - tag.Length];

        Buffer.BlockCopy(input, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(input, nonce.Length, tag, 0, tag.Length);
        Buffer.BlockCopy(input, nonce.Length + tag.Length, actualCipher, 0, actualCipher.Length);

        var plaintext = new byte[actualCipher.Length];

        using (var aes = new AesGcm(key))
        {
            aes.Decrypt(nonce, actualCipher, tag, plaintext, null);
        }

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }
}
