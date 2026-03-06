namespace CookieCommerce.Web.Models.Addresses;

public class AddressBookItemDto
{
    public int Index { get; set; }
    public string Icon { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string HouseNumber { get; set; } = null!;
    public string HouseNumberAddition { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public bool IsDefault { get; set; } = false;
}
