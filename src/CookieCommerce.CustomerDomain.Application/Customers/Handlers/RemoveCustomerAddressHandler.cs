using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class RemoveCustomerAddressHandler(IDocumentSession session) : IRequestHandler<RemoveCustomerAddress>
{
    public async Task Handle(RemoveCustomerAddress request, CancellationToken cancellationToken)
    {
        if (await session.Events.AggregateStreamAsync<CustomerAggregate>(request.CustomerId, token: cancellationToken) 
            is not CustomerAggregate customer)
            throw new Exception($"Customer with ID {request.CustomerId} not found.");

        customer.RemoveAddress(request.Label);
        session.Events.Append(customer.Id, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
