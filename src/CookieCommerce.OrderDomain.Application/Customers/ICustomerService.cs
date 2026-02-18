namespace CookieCommerce.OrderDomain.Application.Customers;

/// <summary>
/// Service for retrieving customer information from the Customer API.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Gets a customer by their unique identifier.
    /// </summary>
    /// <param name="customerId">The customer's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer information if found, null otherwise</returns>
    Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
