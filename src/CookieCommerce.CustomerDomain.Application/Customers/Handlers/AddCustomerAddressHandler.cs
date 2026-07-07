using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Handlers;

public class AddCustomerAddressHandler(
    IDocumentSession session,
    ICustomerKeyStore customerKeyStore,
    CustomerAddressEncryption addressEncryption,
    CustomerAddressValidator addressValidator) : IRequestHandler<AddCustomerAddress>
{
    public async Task Handle(AddCustomerAddress request, CancellationToken cancellationToken)
    {
        if (await session.Events.AggregateStreamAsync<CustomerAggregate>(request.CustomerId, token: cancellationToken)
            is not CustomerAggregate customer)
            throw new CustomerNotFoundException(request.CustomerId);

        var validatedAddress = addressValidator.Validate(
            request.Icon,
            request.Label,
            request.FirstName,
            request.LastName,
            request.Street,
            request.HouseNumber,
            request.HouseNumberAddition,
            request.City,
            request.PostalCode,
            request.Province,
            request.Country,
            request.Phone,
            request.IsDefault,
            customer.Addresses);

        var customerKey = await customerKeyStore.GetKeyAsync(request.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(request.CustomerId);
        var encryptedAddress = addressEncryption.Encrypt(customerKey, validatedAddress);

        customer.AddAddress(new CustomerAddressAdded
        {
            CustomerId = request.CustomerId,
            AddressId = Guid.CreateVersion7(),
            Icon = validatedAddress.Icon,
            Label = validatedAddress.Label,
            EncryptedFirstName = encryptedAddress.EncryptedFirstName,
            EncryptedLastName = encryptedAddress.EncryptedLastName,
            EncryptedStreet = encryptedAddress.EncryptedStreet,
            EncryptedHouseNumber = encryptedAddress.EncryptedHouseNumber,
            EncryptedHouseNumberAddition = encryptedAddress.EncryptedHouseNumberAddition,
            EncryptedCity = encryptedAddress.EncryptedCity,
            EncryptedPostalCode = encryptedAddress.EncryptedPostalCode,
            EncryptedProvince = encryptedAddress.EncryptedProvince,
            EncryptedCountry = encryptedAddress.EncryptedCountry,
            EncryptedPhone = encryptedAddress.EncryptedPhone,
            IsDefault = validatedAddress.IsDefault,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        session.Events.Append(customer.Id, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
