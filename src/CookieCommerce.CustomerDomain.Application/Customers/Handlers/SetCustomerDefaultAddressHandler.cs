using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public sealed class SetCustomerDefaultAddressHandler(IDocumentSession session)
    : IRequestHandler<SetCustomerDefaultAddress>
{
    public async Task Handle(SetCustomerDefaultAddress request, CancellationToken cancellationToken)
    {
        var customer = await session.Events.AggregateStreamAsync<CustomerAggregate>(
            request.CustomerId,
            token: cancellationToken)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        customer.ClearUncommitedEvents();
        customer.SetDefaultAddress(request.AddressId, request.ChangedAtUtc);

        session.Events.Append(customer.Id, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
