namespace CookieCommerce.CustomerDomain.Application.Customers.ReadModels;

public class CustomerAddressReadModel
{
    public Guid AddressId { get; set; }
    public int Index { get; set; }
    public string? Icon { get; set; }
    public string Label { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string HouseNumber { get; set; } = null!;
    public string? HouseNumberAddition { get; set; }
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public bool IsDefault { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    public string State
    {
        get => Province;
        set => Province = value;
    }
}
