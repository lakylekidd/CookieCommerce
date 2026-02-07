using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CustomerDomain.Infrastructure.Projections;

public class CustomerProjection(IServiceCollection services) : EventProjection
{
    private readonly IServiceCollection _services = services;

    public override ValueTask ApplyAsync(IDocumentOperations operations, IEvent @event, CancellationToken cancellation)
    {
        switch (@event.Data)
        {
            case CustomerCreated e:
                Apply(operations, e, cancellation);
                break;
            case CustomerForgotten e:
                Apply(operations, e, cancellation);
                break;
            default:
                throw new InvalidOperationException($"Unknown event type: {@event.GetType().FullName}");
        }

        return new ValueTask();
    }

    private void Apply(IDocumentOperations ops, CustomerCreated e, CancellationToken cancellationToken)
    {
        var services = _services.BuildServiceProvider();
        var keyStore = services.GetRequiredService<ICustomerKeyStore>();
        var encryptionService = services.GetRequiredService<IEncryptionService>();

        var customerKey = keyStore.GetKeyAsync(e.CustomerId, cancellationToken).Result;

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

    private void Apply(IDocumentOperations ops, CustomerForgotten e, CancellationToken cancellationToken)
    {
        ops.DeleteWhere<CustomerReadModel>(c => c.Id == e.CustomerId);
    }
}
