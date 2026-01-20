using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class ForgetCustomerHandler(
    IDocumentSession session, 
    ICustomerKeyStore keyStore,
    IIntegrationEventPublisher publisher) : IRequestHandler<ForgetCustomer>
{
    private readonly IDocumentSession _session = session;
    private readonly ICustomerKeyStore _keyStore = keyStore;
    private readonly IIntegrationEventPublisher _publisher = publisher;

    public async Task Handle(ForgetCustomer request, CancellationToken cancellationToken)
    {
        await _keyStore.DeleteKeyAsync(request.CustomerId, cancellationToken);
        _session.DeleteWhere<CustomerReadModel>(c => c.Id == request.CustomerId);

        var customer = await _session.Events.AggregateStreamAsync<CustomerAggregate>(request.CustomerId, token: cancellationToken);
        if (customer is null) return;

        customer.Delete();

        _session.Events.Append(request.CustomerId, customer.DomainEvents);
        await _session.SaveChangesAsync(cancellationToken);

        await _publisher.PublishAsync(new CustomerDeletedIntegrationEvent(request.CustomerId), cancellationToken);
    }
}
