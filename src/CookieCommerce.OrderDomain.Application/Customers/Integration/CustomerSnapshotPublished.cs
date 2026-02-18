namespace CookieCommerce.OrderDomain.Application.Customers.Integration;

/// <summary>
/// Integration event received when customer data is created or updated.
/// Mirrors CookieCommerce.CustomerDomain.Application.Customers.Integration.CustomerSnapshotPublished
/// </summary>
public class CustomerSnapshotPublished
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
}
