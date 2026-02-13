using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using Marten;
using Marten.Events.Aggregation;

namespace CookieCommerce.CustomerDomain.Infrastructure.Projections;

public class CustomerProjection : SingleStreamProjection<CustomerReadModel, Guid>
{
    private readonly IEncryptionService _encryptionService;

    public CustomerProjection(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
        
        // Define inline projection methods
        DeleteEvent<CustomerForgotten>();
    }

    public async Task<CustomerReadModel> Create(CustomerCreated @event, IQuerySession session)
    {
        // Load the encryption key
        var keyRecord = await session.LoadAsync<CustomerKeyRecord>(@event.CustomerId);
        
        if (keyRecord?.Key == null)
        {
            // Key doesn't exist or was shredded - return null to skip this projection
            return null!;
        }

        var email = _encryptionService.Decrypt(@event.EncryptedEmail, keyRecord.Key);
        var firstName = _encryptionService.Decrypt(@event.EncryptedFirstName, keyRecord.Key);
        var lastName = _encryptionService.Decrypt(@event.EncryptedLastName, keyRecord.Key);
        var gender = _encryptionService.Decrypt(@event.EncryptedGender, keyRecord.Key);
        var dateOfBirth = @event.EncryptedDateOfBirth is not null
            ? _encryptionService.Decrypt(@event.EncryptedDateOfBirth, keyRecord.Key)
            : null;

        return new CustomerReadModel
        {
            Id = @event.CustomerId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Gender = Enum.Parse<Gender>(gender),
            DateOfBirth = dateOfBirth is not null
                ? DateTime.Parse(dateOfBirth)
                : null,
            Addresses = new List<CustomerAddressReadModel>()
        };
    }
    
    public CustomerReadModel Apply(CustomerAddressAdded @event, CustomerReadModel current)
    {
        // Add address to the read model  
        var address = new CustomerAddressReadModel
        {
            Label = @event.Address.Label,
            Street = @event.Address.Street,
            City = @event.Address.City,
            State = "", // Not available in Address value object
            PostalCode = @event.Address.PostalCode,
            Country = @event.Address.CountryIso2LetterCode
        };
        
        current.Addresses.Add(address);
        return current;
    }
    
    public CustomerReadModel Apply(CustomerAddressRemoved @event, CustomerReadModel current)
    {
        var addressToRemove = current.Addresses.FirstOrDefault(a => a.Label == @event.Label);
        if (addressToRemove != null)
        {
            current.Addresses.Remove(addressToRemove);
        }
        return current;
    }
}
