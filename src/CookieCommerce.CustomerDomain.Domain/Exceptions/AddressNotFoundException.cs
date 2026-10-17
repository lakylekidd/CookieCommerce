namespace CookieCommerce.CustomerDomain.Domain.Exceptions;

/// <summary>
/// Exception thrown when an address with the specified label is not found
/// </summary>
public class AddressNotFoundException : Exception
{
    public Guid CustomerId { get; }
    public string Label { get; }

    public AddressNotFoundException(Guid customerId, string label)
        : base($"Address with label '{label}' does not exist for customer '{customerId}'.")
    {
        CustomerId = customerId;
        Label = label;
    }
}
