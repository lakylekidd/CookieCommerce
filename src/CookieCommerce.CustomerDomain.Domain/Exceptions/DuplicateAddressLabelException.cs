namespace CookieCommerce.CustomerDomain.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to add an address with a label that already exists
/// </summary>
public class DuplicateAddressLabelException : Exception
{
    public Guid CustomerId { get; }
    public string Label { get; }

    public DuplicateAddressLabelException(Guid customerId, string label) 
        : base($"An address with the label '{label}' already exists for customer '{customerId}'.")
    {
        CustomerId = customerId;
        Label = label;
    }
}
