using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class AddCustomerAddressHandler(IDocumentSession session) : IRequestHandler<AddCustomerAddress>
{
    public async Task Handle(AddCustomerAddress request, CancellationToken cancellationToken)
    {
        if (await session.Events.AggregateStreamAsync<CustomerAggregate>(request.CustomerId,token: cancellationToken) 
            is not CustomerAggregate customer)
            throw new CustomerNotFoundException(request.CustomerId);

        var address = new Address(request.Label, 
            request.Street, 
            request.City,
            request.PostalCode, 
            request.Country,
            request.IsPrimary);
        customer.AddAddress(address);

        session.Events.Append(customer.Id, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
