using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Checkout;

namespace CookieCommerce.Web.Services.StaticData;

public static class StaticAddressList
{
    public static AddressDto Address001 = new()
    {
        Street = "Baker Street",
        HouseNumber = "221",
        HouseNumberAddition = "B",
        City = "Amsterdam",
        PostalCode = "1011 AB",
        Province = "North Holland",
        Country = "The Netherlands"
    };
    public static AddressDto Address002 = new()
    {
        Street = "Zuidas Financial District",
        HouseNumber = "42",
        City = "Amsterdam",
        PostalCode = "1082 MS",
        Province = "North Holland",
        Country = "The Netherlands"
    };
    public static AddressDto Address003 = new()
    {
        Street = "Strandweg",
        HouseNumber = "15",
        City = "Den Haag",
        PostalCode = "2586 JK",
        Province = "South Holland",
        Country = "The Netherlands"
    };
    public static AddressDto Address004 = new()
    {
        Street = "Lange Voorhout",
        HouseNumber = "20",
        City = "Den Haag",
        PostalCode = "2514 EJ",
        Province = "South Holland",
        Country = "The Netherlands"
    };
    public static AddressDto Address005 = new()
    {
        Street = "Keizersgracht",
        HouseNumber = "123",
        City = "Amsterdam",
        PostalCode = "1015 CJ",
        Province = "North Holland",
        Country = "The Netherlands"
    };

    public static AddressBookItemDto ToAddressBookItem(this AddressDto address, 
        int index, string label, string phone, 
        string firstName, string lastName, 
        string icon, bool isDefault = false)
    {
        return new AddressBookItemDto
        {
            Index = index,
            Label = label,
            Phone = phone,

            FirstName = firstName,
            LastName = lastName,
            Icon = icon,
            IsDefault = isDefault,

            Street = address.Street,
            HouseNumber = address.HouseNumber,
            HouseNumberAddition = address.HouseNumberAddition,
            City = address.City,
            PostalCode = address.PostalCode,
            Province = address.Province,
            Country = address.Country
        };
    }
}
