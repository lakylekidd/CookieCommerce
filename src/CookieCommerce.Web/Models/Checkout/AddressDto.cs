namespace CookieCommerce.Web.Models.Checkout;

public class AddressDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Street { get; set; } = default!;
    public string HouseNumber { get; set; } = default!;
    public string? HouseNumberAddition { get; set; }
    public string City { get; set; } = default!;
    public string Province { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;
}
