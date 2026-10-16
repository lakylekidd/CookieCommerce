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
            CreatedAtUtc = @event.CreatedAtUtc,
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

    public async Task<CustomerReadModel> Apply(CustomerAddressAdded @event, CustomerReadModel current, IQuerySession session)
    {
        var keyRecord = await session.LoadAsync<CustomerKeyRecord>(@event.CustomerId);

        if (keyRecord?.Key == null)
        {
            return current;
        }

        if (@event.IsDefault || current.Addresses.Count == 0)
        {
            foreach (var existingAddress in current.Addresses)
            {
                existingAddress.IsDefault = false;
            }
        }

        var address = new CustomerAddressReadModel
        {
            AddressId = @event.AddressId,
            Index = current.Addresses.Count + 1,
            Icon = @event.Icon,
            Label = @event.Label,
            FirstName = _encryptionService.Decrypt(@event.EncryptedFirstName, keyRecord.Key),
            LastName = _encryptionService.Decrypt(@event.EncryptedLastName, keyRecord.Key),
            Street = _encryptionService.Decrypt(@event.EncryptedStreet, keyRecord.Key),
            HouseNumber = _encryptionService.Decrypt(@event.EncryptedHouseNumber, keyRecord.Key),
            HouseNumberAddition = @event.EncryptedHouseNumberAddition is null
                ? null
                : _encryptionService.Decrypt(@event.EncryptedHouseNumberAddition, keyRecord.Key),
            City = _encryptionService.Decrypt(@event.EncryptedCity, keyRecord.Key),
            PostalCode = _encryptionService.Decrypt(@event.EncryptedPostalCode, keyRecord.Key),
            Province = _encryptionService.Decrypt(@event.EncryptedProvince, keyRecord.Key),
            Country = _encryptionService.Decrypt(@event.EncryptedCountry, keyRecord.Key),
            Phone = _encryptionService.Decrypt(@event.EncryptedPhone, keyRecord.Key),
            IsDefault = @event.IsDefault || current.Addresses.Count == 0,
            CreatedAtUtc = @event.CreatedAtUtc
        };

        current.Addresses.Add(address);
        ReindexAddresses(current);
        return current;
    }

    public CustomerReadModel Apply(CustomerAddressRemoved @event, CustomerReadModel current)
    {
        var addressToRemove = @event.AddressId == Guid.Empty
            ? current.Addresses.FirstOrDefault()
            : current.Addresses.FirstOrDefault(a => a.AddressId == @event.AddressId);
        if (addressToRemove != null)
            current.Addresses.Remove(addressToRemove);

        if (current.Addresses.Count > 0 && current.Addresses.All(x => !x.IsDefault))
        {
            current.Addresses.OrderBy(x => x.CreatedAtUtc).First().IsDefault = true;
        }

        ReindexAddresses(current);
        return current;
    }

    public CustomerReadModel Apply(CustomerDefaultAddressChanged @event, CustomerReadModel current)
    {
        foreach (var address in current.Addresses)
        {
            address.IsDefault = address.AddressId == @event.AddressId;
        }

        ReindexAddresses(current);
        return current;
    }

    private static void ReindexAddresses(CustomerReadModel current)
    {
        var index = 1;
        foreach (var address in current.Addresses.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.AddressId))
        {
            address.Index = index++;
        }
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
