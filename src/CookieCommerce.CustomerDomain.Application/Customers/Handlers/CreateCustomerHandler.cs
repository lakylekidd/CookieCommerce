using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.DomainBuildingBlocks;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class CreateCustomerHandler(
    IDocumentSession session,
    ICustomerKeyStore keyStore,
    IEncryptionService encryptionService,
    IIntegrationEventPublisher publisher) : IRequestHandler<CreateCustomer, Guid>
{
    public async Task<Guid> Handle(CreateCustomer request, CancellationToken cancellationToken)
    {
        var customerId = request.CustomerId ?? Guid.CreateVersion7();

        var key = await keyStore.CreateKeyForCustomerAsync(customerId, cancellationToken);

        var encryptedEmail = encryptionService.Encrypt(request.Email, key);
        var encryptedFirstName = encryptionService.Encrypt(request.FirstName, key);
        var encryptedLastName = encryptionService.Encrypt(request.LastName, key);
        var encryptedGender = encryptionService.Encrypt(request.Gender.ToString(), key);
        var encryptedDateOfBirth = request.DateOfBirth != null
            ? encryptionService.Encrypt(request.DateOfBirth.Value.ToString("O"), key)
            : null;

        var customer = new CustomerAggregate();
        customer.Create(customerId, encryptedEmail, encryptedFirstName, encryptedLastName, encryptedGender, encryptedDateOfBirth);

        session.Events.StartStream<CustomerAggregate>(customerId, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        // Publish integration event for other domains to consume
        await publisher.PublishAsync(new CustomerSnapshotPublished
        {
            CustomerId = customerId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        }, cancellationToken);

        return customerId;
    }
}
