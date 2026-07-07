namespace CookieCommerce.CustomerDomain.Domain.Exceptions;

/// <summary>
/// Exception thrown when a customer is not found
/// </summary>
public class CustomerNotFoundException : Exception
{
    public Guid CustomerId { get; }

    public CustomerNotFoundException(Guid customerId)
        : base($"Customer with ID '{customerId}' was not found.")
    {
        CustomerId = customerId;
    }

    public CustomerNotFoundException(Guid customerId, string message)
        : base(message)
    {
        CustomerId = customerId;
    }
}
