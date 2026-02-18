namespace CookieCommerce.OrderDomain.Application.Customers;

/// <summary>
/// Data transfer object for customer information from the Customer API.
/// </summary>
public class CustomerDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
