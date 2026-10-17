using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Application.Customers.Profile;

public sealed class CustomerProfileEncryption
{
    private readonly IEncryptionService _encryptionService;

    public CustomerProfileEncryption(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public EncryptedCustomerProfile Encrypt(
        byte[] customerKey,
        string firstName,
        string lastName,
        string verifiedEmail,
        string? pendingEmail,
        CustomerProfileGender? gender,
        DateOnly? dateOfBirth)
    {
        return new EncryptedCustomerProfile(
            _encryptionService.Encrypt(firstName, customerKey),
            _encryptionService.Encrypt(lastName, customerKey),
            _encryptionService.Encrypt(verifiedEmail, customerKey),
            pendingEmail is null ? null : _encryptionService.Encrypt(pendingEmail, customerKey),
            gender is null ? null : _encryptionService.Encrypt(gender.Value.ToString(), customerKey),
            dateOfBirth is null ? null : _encryptionService.Encrypt(dateOfBirth.Value.ToString("O"), customerKey));
    }
}

public sealed record EncryptedCustomerProfile(
    string EncryptedFirstName,
    string EncryptedLastName,
    string EncryptedVerifiedEmail,
    string? EncryptedPendingEmail,
    string? EncryptedGender,
    string? EncryptedDateOfBirth);
