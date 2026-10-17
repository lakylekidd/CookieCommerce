namespace CookieCommerce.CustomerDomain.Domain.ValueObjects;

public class Address
{
    public Guid AddressId { get; }
    public string? Icon { get; }
    public string Label { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Street { get; }
    public string HouseNumber { get; }
    public string? HouseNumberAddition { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Province { get; }
    public string Country { get; }
    public string Phone { get; }
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    public string CountryIso2LetterCode => Country;
    public bool IsPrimary => IsDefault;

    public Address(
        Guid addressId,
        string? icon,
        string label,
        string firstName,
        string lastName,
        string street,
        string houseNumber,
        string? houseNumberAddition,
        string city,
        string postalCode,
        string province,
        string country,
        string phone,
        bool isDefault,
        DateTimeOffset createdAtUtc)
    {
        AddressId = addressId;
        Icon = icon;
        Label = label;
        FirstName = firstName;
        LastName = lastName;
        Street = street;
        HouseNumber = houseNumber;
        HouseNumberAddition = houseNumberAddition;
        City = city;
        PostalCode = postalCode;
        Province = province;
        Country = country;
        Phone = phone;
        IsDefault = isDefault;
        CreatedAtUtc = createdAtUtc;
    }

    public Address(
        string label,
        string street,
        string city,
        string postalCode,
        string countryIso2LetterCode,
        bool isPrimary = true)
        : this(
            Guid.CreateVersion7(),
            null,
            label,
            string.Empty,
            string.Empty,
            street,
            string.Empty,
            null,
            city,
            postalCode,
            string.Empty,
            countryIso2LetterCode,
            string.Empty,
            isPrimary,
            DateTimeOffset.UtcNow)
    {
    }

    public Address WithDefault(bool isDefault) =>
        new(
            AddressId,
            Icon,
            Label,
            FirstName,
            LastName,
            Street,
            HouseNumber,
            HouseNumberAddition,
            City,
            PostalCode,
            Province,
            Country,
            Phone,
            isDefault,
            CreatedAtUtc);

    public void UnsetPrimary()
    {
        IsDefault = false;
    }
}
