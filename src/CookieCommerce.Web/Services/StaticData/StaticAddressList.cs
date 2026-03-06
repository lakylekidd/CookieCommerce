using CookieCommerce.Web.Models.Addresses;

namespace CookieCommerce.Web.Services.StaticData;

public static class StaticAddressList
{
    public static List<AddressBookItemDto> Addresses = new List<AddressBookItemDto>
    {
        new()
        {
            Index = 0,
            Icon = "home",
            Label = "Home Base",
            FirstName = "Cookie",
            LastName = "Lover",
            Street = "Baker Street",
            HouseNumber = "221",
            HouseNumberAddition = "B",
            City = "Amsterdam",
            PostalCode = "1011 AB",
            State = "North Holland",
            Country = "The Netherlands",
            Phone = "+31 6 1234 5678",
            IsDefault = true
        },
        new()
        {
            Index = 1,
            Icon = "work",
            Label = "Cookie HQ",
            FirstName = "Cookie",
            LastName = "Lover (Office)",
            Street = "Zuidas Financial District",
            HouseNumber = "42",
            City = "Amsterdam",
            PostalCode = "1082 MS",
            State = "North Holland",
            Country = "The Netherlands",
            Phone = "+31 6 8765 4321"
        },
        new()
        {
            Index = 2,
            Icon = "carry_on_bag",
            Label = "Summer Den",
            FirstName = "Cookie",
            LastName = "Lover",
            Street = "Strandweg",
            HouseNumber = "15",
            City = "Den Haag",
            PostalCode = "2586 JK",
            State = "South Holland",
            Country = "The Netherlands",
            Phone = "",
            IsDefault = false
        }
    };
}
