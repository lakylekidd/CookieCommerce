namespace CookieCommerce.CustomerDomain.Domain.ValueObjects;

public class Address
{
    public string Label { get; }
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string CountryIso2LetterCode { get; }
    public bool IsPrimary { get; private set; }

    public Address(
        string label,
        string street,
        string city,
        string postalCode,
        string countryIso2LetterCode,
        bool isPrimary = true)
    {
        Label = label;
        Street = street;
        City = city;
        PostalCode = postalCode;
        CountryIso2LetterCode = countryIso2LetterCode;
        IsPrimary = isPrimary;
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
    }
}
