using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Commands;

public class RemoveCustomerAddress(Guid customerId, Guid addressId, DateTimeOffset deletedAtUtc) : IRequest
{
    public Guid CustomerId { get; } = customerId;
    public Guid AddressId { get; } = addressId;
    public DateTimeOffset DeletedAtUtc { get; } = deletedAtUtc;
}

public class RemoveCustomerAddressByLabel(Guid customerId, string label, DateTimeOffset deletedAtUtc) : IRequest
{
    public Guid CustomerId { get; } = customerId;
    public string Label { get; } = label;
    public DateTimeOffset DeletedAtUtc { get; } = deletedAtUtc;
}
