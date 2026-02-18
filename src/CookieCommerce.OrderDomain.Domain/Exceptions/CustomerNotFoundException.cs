namespace CookieCommerce.OrderDomain.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to place an order for a customer that does not exist.
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

    public CustomerNotFoundException(Guid customerId, string message, Exception innerException)
        : base(message, innerException)
    {
        CustomerId = customerId;
    }
}
