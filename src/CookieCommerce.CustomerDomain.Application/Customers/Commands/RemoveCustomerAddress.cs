using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public class RemoveCustomerAddress(Guid customerId, string label) : IRequest
{
    public Guid CustomerId { get; } = customerId;
    public string Label { get; } = label;
}
