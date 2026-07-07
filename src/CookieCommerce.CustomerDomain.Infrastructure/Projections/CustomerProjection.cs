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

        // Crypto-shred: delete read model on customer forgotten/deletion
        DeleteEvent<CustomerForgotten>();
    }

    public async Task<CustomerReadModel> Create(CustomerCreated @event, IQuerySession session)
    {
        var keyRecord = await session.LoadAsync<CustomerKeyRecord>(@event.CustomerId);

        if (keyRecord?.Key == null)
        {
            // Key doesn't exist or was shredded — skip this projection
            return null!;
        }

        var email = _encryptionService.Decrypt(@event.EncryptedEmail, keyRecord.Key);
        var firstName = _encryptionService.Decrypt(@event.EncryptedFirstName, keyRecord.Key);
        var lastName = _encryptionService.Decrypt(@event.EncryptedLastName, keyRecord.Key);
        var genderStr = _encryptionService.Decrypt(@event.EncryptedGender, keyRecord.Key);
        var gender = ParseGender(genderStr);

        DateOnly? dateOfBirth = null;
        if (@event.EncryptedDateOfBirth is not null)
        {
            var dobStr = _encryptionService.Decrypt(@event.EncryptedDateOfBirth, keyRecord.Key);
            if (DateOnly.TryParse(dobStr, out var dob))
                dateOfBirth = dob;
        }

        return new CustomerReadModel
        {
            Id = @event.CustomerId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            EmailVerificationStatus = "Verified",
            Addresses = new List<CustomerAddressReadModel>()
        };
    }

    public async Task<CustomerReadModel> Apply(CustomerProfileUpdated @event, CustomerReadModel current, IQuerySession session)
    {
        var keyRecord = await session.LoadAsync<CustomerKeyRecord>(@event.CustomerId);

        if (keyRecord?.Key == null)
        {
            // Key shredded — cannot decrypt; leave existing values intact
            return current;
        }

        current.FirstName = _encryptionService.Decrypt(@event.EncryptedFirstName, keyRecord.Key);
        current.LastName = _encryptionService.Decrypt(@event.EncryptedLastName, keyRecord.Key);

        if (@event.EncryptedGender is not null)
        {
            var genderStr = _encryptionService.Decrypt(@event.EncryptedGender, keyRecord.Key);
            current.Gender = ParseGender(genderStr);
        }
        else
        {
            current.Gender = null;
        }

        if (@event.EncryptedDateOfBirth is not null)
        {
            var dobStr = _encryptionService.Decrypt(@event.EncryptedDateOfBirth, keyRecord.Key);
            current.DateOfBirth = DateOnly.TryParse(dobStr, out var dob) ? dob : null;
        }
        else
        {
            current.DateOfBirth = null;
        }

        return current;
    }

    public async Task<CustomerReadModel> Apply(CustomerEmailChangeRequested @event, CustomerReadModel current, IQuerySession session)
    {
        var keyRecord = await session.LoadAsync<CustomerKeyRecord>(@event.CustomerId);

        if (keyRecord?.Key != null)
        {
            current.PendingEmail = _encryptionService.Decrypt(@event.EncryptedPendingEmail, keyRecord.Key);
        }

        current.EmailVerificationStatus = "PendingVerification";
        return current;
    }

    public async Task<CustomerReadModel> Apply(CustomerVerifiedEmailChanged @event, CustomerReadModel current, IQuerySession session)
    {
        var keyRecord = await session.LoadAsync<CustomerKeyRecord>(@event.CustomerId);

        if (keyRecord?.Key != null)
        {
            current.Email = _encryptionService.Decrypt(@event.EncryptedVerifiedEmail, keyRecord.Key);
        }

        current.PendingEmail = null;
        current.EmailVerificationStatus = "Verified";
        return current;
    }

    public CustomerReadModel Apply(CustomerAddressAdded @event, CustomerReadModel current)
    {
        var address = new CustomerAddressReadModel
        {
            Label = @event.Address.Label,
            Street = @event.Address.Street,
            City = @event.Address.City,
            State = string.Empty,
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
            current.Addresses.Remove(addressToRemove);

        return current;
    }

    private static CustomerProfileGender? ParseGender(string? genderStr)
    {
        if (string.IsNullOrWhiteSpace(genderStr))
            return null;

        return genderStr switch
        {
            "Male" => CustomerProfileGender.Male,
            "Female" => CustomerProfileGender.Female,
            "PreferNotToDisclose" => CustomerProfileGender.PreferNotToDisclose,
            // Legacy values from older Gender enum
            "1" => CustomerProfileGender.Male,
            "2" => CustomerProfileGender.Female,
            _ => null
        };
    }
}
