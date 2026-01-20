namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerAddressRemoved
{
    public Guid CustomerId { get; }
    public string Label { get; }

    public CustomerAddressRemoved(Guid customerId, string label)
    {
        CustomerId = customerId;
        Label = label;
    }
}
