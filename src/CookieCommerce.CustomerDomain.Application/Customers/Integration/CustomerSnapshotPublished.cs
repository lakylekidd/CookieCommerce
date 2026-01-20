namespace CookieCommerce.CustomerDomain.Application.Customers.Integration;

public class CustomerSnapshotPublished
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; } = null;
}
