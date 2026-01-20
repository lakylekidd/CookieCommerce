namespace CookieCommerce.CustomerDomain.Application.Security;

public interface IEncryptionService
{
    string Encrypt(string plainText, byte[] key);
    string Decrypt(string ciphertext, byte[] key);
}
