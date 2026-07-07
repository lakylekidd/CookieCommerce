using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;

namespace CookieCommerce.CustomerDomain.Application.Customers.Profile;

public sealed class CustomerAddressEncryption
{
    private readonly IEncryptionService _encryptionService;

    public CustomerAddressEncryption(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public EncryptedCustomerAddress Encrypt(byte[] customerKey, ValidatedCustomerAddress address) =>
        new(
            _encryptionService.Encrypt(address.FirstName, customerKey),
            _encryptionService.Encrypt(address.LastName, customerKey),
            _encryptionService.Encrypt(address.Street, customerKey),
            _encryptionService.Encrypt(address.HouseNumber, customerKey),
            string.IsNullOrWhiteSpace(address.HouseNumberAddition)
                ? null
                : _encryptionService.Encrypt(address.HouseNumberAddition, customerKey),
            _encryptionService.Encrypt(address.City, customerKey),
            _encryptionService.Encrypt(address.PostalCode, customerKey),
            _encryptionService.Encrypt(address.Province, customerKey),
            _encryptionService.Encrypt(address.Country, customerKey),
            _encryptionService.Encrypt(address.Phone, customerKey));

    public DecryptedCustomerAddress Decrypt(byte[] customerKey, CustomerAddressAdded address) =>
        new(
            _encryptionService.Decrypt(address.EncryptedFirstName, customerKey),
            _encryptionService.Decrypt(address.EncryptedLastName, customerKey),
            _encryptionService.Decrypt(address.EncryptedStreet, customerKey),
            _encryptionService.Decrypt(address.EncryptedHouseNumber, customerKey),
            address.EncryptedHouseNumberAddition is null
                ? null
                : _encryptionService.Decrypt(address.EncryptedHouseNumberAddition, customerKey),
            _encryptionService.Decrypt(address.EncryptedCity, customerKey),
            _encryptionService.Decrypt(address.EncryptedPostalCode, customerKey),
            _encryptionService.Decrypt(address.EncryptedProvince, customerKey),
            _encryptionService.Decrypt(address.EncryptedCountry, customerKey),
            _encryptionService.Decrypt(address.EncryptedPhone, customerKey));
}

public sealed record EncryptedCustomerAddress(
    string EncryptedFirstName,
    string EncryptedLastName,
    string EncryptedStreet,
    string EncryptedHouseNumber,
    string? EncryptedHouseNumberAddition,
    string EncryptedCity,
    string EncryptedPostalCode,
    string EncryptedProvince,
    string EncryptedCountry,
    string EncryptedPhone);

public sealed record DecryptedCustomerAddress(
    string FirstName,
    string LastName,
    string Street,
    string HouseNumber,
    string? HouseNumberAddition,
    string City,
    string PostalCode,
    string Province,
    string Country,
    string Phone);
