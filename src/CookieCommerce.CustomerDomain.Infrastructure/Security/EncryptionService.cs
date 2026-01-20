using CookieCommerce.CustomerDomain.Application.Security;

namespace CookieCommerce.CustomerDomain.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    public string Decrypt(string ciphertext, byte[] key)
    {
        var bytes = Convert.FromBase64String(ciphertext);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public string Encrypt(string plainText, byte[] key)
    {
        // TODO: Simplified example - in real scenario use AES-GCM, IV, etc.
        var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }
}
