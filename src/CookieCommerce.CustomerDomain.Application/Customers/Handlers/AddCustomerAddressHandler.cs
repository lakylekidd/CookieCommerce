using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
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
    CustomerAddressValidator addressValidator) : IRequestHandler<AddCustomerAddress, CustomerAddressReadModel>
{
    public async Task<CustomerAddressReadModel> Handle(AddCustomerAddress request, CancellationToken cancellationToken)
    {
        if (await session.Events.AggregateStreamAsync<CustomerAggregate>(request.CustomerId, token: cancellationToken)
            is not CustomerAggregate customer)
            throw new CustomerNotFoundException(request.CustomerId);

        customer.ClearUncommitedEvents();

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

        if (customer.Addresses.Any(a => string.Equals(a.Label, validatedAddress.Label, StringComparison.OrdinalIgnoreCase)))
            throw new DuplicateCustomerAddressLabelException(request.CustomerId, validatedAddress.Label);

        var customerKey = await customerKeyStore.GetKeyAsync(request.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(request.CustomerId);
        var encryptedAddress = addressEncryption.Encrypt(customerKey, validatedAddress);

        var addressAdded = new CustomerAddressAdded
        {
            CustomerId = request.CustomerId,
            AddressId = request.AddressId,
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
            CreatedAtUtc = request.CreatedAtUtc
        };

        customer.AddAddress(addressAdded);
        session.Events.Append(customer.Id, customer.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        return new CustomerAddressReadModel
        {
            AddressId = addressAdded.AddressId,
            Index = customer.Addresses.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.AddressId).ToList().FindIndex(x => x.AddressId == addressAdded.AddressId) + 1,
            Icon = addressAdded.Icon,
            Label = validatedAddress.Label,
            FirstName = validatedAddress.FirstName,
            LastName = validatedAddress.LastName,
            Street = validatedAddress.Street,
            HouseNumber = validatedAddress.HouseNumber,
            HouseNumberAddition = validatedAddress.HouseNumberAddition,
            City = validatedAddress.City,
            PostalCode = validatedAddress.PostalCode,
            Province = validatedAddress.Province,
            Country = validatedAddress.Country,
            Phone = validatedAddress.Phone,
            IsDefault = customer.Addresses.Single(x => x.AddressId == addressAdded.AddressId).IsDefault,
            CreatedAtUtc = addressAdded.CreatedAtUtc
        };
    }
}
