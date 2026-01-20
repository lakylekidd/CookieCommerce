using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CustomerDomain.Infrastructure.Projections;

public class CustomerProjection(IServiceProvider services) : IProjection
{
    private readonly IServiceProvider _services = services;

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        foreach (var @event in events)
        {
            switch (@event.Data)
            {
                case CustomerCreated e:
                    await Apply(operations, e, cancellation);
                    break;
                case CustomerDeleted e:
                    await Apply(operations, e, cancellation);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown event type: {@event.GetType().FullName}");
            }
        }
    }

    private async Task Apply(IDocumentOperations ops, CustomerCreated e, CancellationToken cancellationToken)
    {
        var keyStore = _services.GetRequiredService<ICustomerKeyStore>();
        var encryptionService = _services.GetRequiredService<IEncryptionService>();

        var customerKey = await keyStore.GetKeyAsync(e.CustomerId, cancellationToken);

        if (customerKey == null) return; // Key shredded -> treat as deleted/anonymized

        var email = encryptionService.Decrypt(e.EncryptedEmail, customerKey);
        var firstName = encryptionService.Decrypt(e.EncryptedFirstName, customerKey);
        var lastName = encryptionService.Decrypt(e.EncryptedLastName, customerKey);
        var gender = encryptionService.Decrypt(e.EncryptedGender, customerKey);
        var dateOfBirth = e.EncryptedDateOfBirth is not null
            ? encryptionService.Decrypt(e.EncryptedDateOfBirth, customerKey)
            : null;

        var customerReadModel = new CustomerReadModel
        {
            Id = e.CustomerId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Gender = Enum.Parse<Gender>(gender),
            DateOfBirth = dateOfBirth is not null
                ? DateTime.Parse(dateOfBirth)
                : null
        };

        ops.Store(customerReadModel);
    }

    private Task Apply(IDocumentOperations ops, CustomerDeleted e, CancellationToken cancellationToken)
    {
        ops.DeleteWhere<CustomerReadModel>(c => c.Id == e.CustomerId);
        return Task.CompletedTask;
    }
}
