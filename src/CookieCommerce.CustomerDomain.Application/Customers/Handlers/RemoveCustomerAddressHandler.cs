using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class RemoveCustomerAddressHandler(IDocumentSession session) :
    IRequestHandler<RemoveCustomerAddress>,
    IRequestHandler<RemoveCustomerAddressByLabel>
{
    public async Task Handle(RemoveCustomerAddress request, CancellationToken cancellationToken)
    {
        var customer = await LoadCustomerAsync(request.CustomerId, cancellationToken);

        customer.RemoveAddress(request.AddressId, request.DeletedAtUtc);
        session.Events.Append(customer.Id, customer.DomainEvents);
        await UpdateReadModelAsync(request.CustomerId, request.AddressId, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(RemoveCustomerAddressByLabel request, CancellationToken cancellationToken)
    {
        var customer = await LoadCustomerAsync(request.CustomerId, cancellationToken);

        customer.RemoveAddress(request.Label);
        session.Events.Append(customer.Id, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }

    private async Task<CustomerAggregate> LoadCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await session.Events.AggregateStreamAsync<CustomerAggregate>(customerId, token: cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        customer.ClearUncommitedEvents();
        return customer;
    }

    private async Task UpdateReadModelAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken)
    {
        var readModel = await session.LoadAsync<CustomerReadModel>(customerId, cancellationToken);
        var address = readModel?.Addresses.FirstOrDefault(x => x.AddressId == addressId);
        if (readModel is null || address is null)
        {
            return;
        }

        readModel.Addresses.Remove(address);
        if (readModel.Addresses.Count > 0 && readModel.Addresses.All(x => !x.IsDefault))
        {
            readModel.Addresses.OrderBy(x => x.CreatedAtUtc).First().IsDefault = true;
        }

        var index = 1;
        foreach (var item in readModel.Addresses.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.AddressId))
        {
            item.Index = index++;
        }

        session.Store(readModel);
    }
}
