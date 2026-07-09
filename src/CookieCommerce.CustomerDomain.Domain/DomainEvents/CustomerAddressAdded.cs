using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerAddressAdded
{
    public CustomerAddressAdded()
    {
    }

    public CustomerAddressAdded(Guid customerId, Address address)
    {
        CustomerId = customerId;
        AddressId = address.AddressId;
        Icon = address.Icon;
        Label = address.Label;
        EncryptedFirstName = address.FirstName;
        EncryptedLastName = address.LastName;
        EncryptedStreet = address.Street;
        EncryptedHouseNumber = address.HouseNumber;
        EncryptedHouseNumberAddition = address.HouseNumberAddition;
        EncryptedCity = address.City;
        EncryptedPostalCode = address.PostalCode;
        EncryptedProvince = address.Province;
        EncryptedCountry = address.Country;
        EncryptedPhone = address.Phone;
        IsDefault = address.IsDefault;
        CreatedAtUtc = address.CreatedAtUtc;
    }
    public Guid CustomerId { get; set; }
    public Guid AddressId { get; set; }
    public string? Icon { get; set; }
    public string Label { get; set; } = string.Empty;
    public string EncryptedFirstName { get; set; } = string.Empty;
    public string EncryptedLastName { get; set; } = string.Empty;
    public string EncryptedStreet { get; set; } = string.Empty;
    public string EncryptedHouseNumber { get; set; } = string.Empty;
    public string? EncryptedHouseNumberAddition { get; set; }
    public string EncryptedCity { get; set; } = string.Empty;
    public string EncryptedPostalCode { get; set; } = string.Empty;
    public string EncryptedProvince { get; set; } = string.Empty;
    public string EncryptedCountry { get; set; } = string.Empty;
    public string EncryptedPhone { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
