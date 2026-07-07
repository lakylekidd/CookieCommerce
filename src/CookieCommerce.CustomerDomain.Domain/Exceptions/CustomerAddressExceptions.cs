namespace CookieCommerce.CustomerDomain.Domain.Exceptions;

public class CustomerAddressValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public CustomerAddressValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more address validation errors occurred.")
    {
        Errors = errors;
    }
}

public class DuplicateCustomerAddressLabelException : Exception
{
    public Guid CustomerId { get; }
    public string Label { get; }

    public DuplicateCustomerAddressLabelException(Guid customerId, string label)
        : base("An active address with the same label already exists for this customer.")
    {
        CustomerId = customerId;
        Label = label;
    }
}

public class CustomerAddressNotFoundException : Exception
{
    public Guid CustomerId { get; }
    public Guid AddressId { get; }

    public CustomerAddressNotFoundException(Guid customerId, Guid addressId)
        : base("Address was not found for this customer.")
    {
        CustomerId = customerId;
        AddressId = addressId;
    }
}
