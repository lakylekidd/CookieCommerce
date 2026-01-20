namespace CookieCommerce.CustomerDomain.Domain.ValueObjects;

public class Address
{
    public string Label { get; }
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string CountryIso2LetterCode { get; }

    public Address(
        string label,
        string street,
        string city,
        string postalCode,
        string countryIso2LetterCode)
    {
        Label = label;
        Street = street;
        City = city;
        PostalCode = postalCode;
        CountryIso2LetterCode = countryIso2LetterCode;
    }
}
