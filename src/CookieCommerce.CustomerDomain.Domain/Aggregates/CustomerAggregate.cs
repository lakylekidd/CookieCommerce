using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.CustomerDomain.Domain.Aggregates;

public class CustomerAggregate : AggregateBase
{
    private readonly List<Address> _addresses = [];
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    public string EncryptedFirstName { get; private set; } = default!;
    public string EncryptedLastName { get; private set; } = default!;
    public string EncryptedGender { get; private set; } = default!;
    public string? EncryptedDateOfBirth { get; private set; } = default!;
    public string EncryptedEmail { get; private set; } = default!;
    public string? EncryptedPendingEmail { get; private set; }
    public string? VerificationTokenReference { get; private set; }
    public DateTimeOffset? VerificationExpiresAtUtc { get; private set; }
    public DateTime? ForgottenDateUtc { get; private set; } = null;

    public CustomerAggregate() { }

    public void Create(Guid id,
        string encryptedEmail,
        string encryptedFirstName,
        string encryptedLastName,
        string encryptedGender,
        string? encryptedDateOfBirth)
    {
        if (string.IsNullOrEmpty(encryptedEmail))
            throw new Exception("E-mail is required.");

        if (string.IsNullOrEmpty(encryptedFirstName))
            throw new Exception("First name is required.");

        if (string.IsNullOrEmpty(encryptedLastName))
            throw new Exception("Last name is required.");

        if (string.IsNullOrEmpty(encryptedGender))
            throw new Exception("Gender is required.");

        Apply(new CustomerCreated
        {
            CustomerId = id,
            EncryptedEmail = encryptedEmail,
            EncryptedFirstName = encryptedFirstName,
            EncryptedLastName = encryptedLastName,
            EncryptedGender = encryptedGender,
            EncryptedDateOfBirth = encryptedDateOfBirth
        });
    }

    public void UpdateProfile(
        string encryptedFirstName,
        string encryptedLastName,
        string? encryptedGender,
        string? encryptedDateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(encryptedFirstName))
            throw new CustomerProfileValidationException("First name is required.");

        if (string.IsNullOrWhiteSpace(encryptedLastName))
            throw new CustomerProfileValidationException("Last name is required.");

        Apply(new CustomerProfileUpdated
        {
            CustomerId = Id,
            EncryptedFirstName = encryptedFirstName,
            EncryptedLastName = encryptedLastName,
            EncryptedGender = encryptedGender,
            EncryptedDateOfBirth = encryptedDateOfBirth,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
    }

    public void RequestEmailChange(
        string encryptedPendingEmail,
        string verificationTokenReference,
        DateTimeOffset requestedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(encryptedPendingEmail))
            throw new CustomerProfileValidationException("Pending email is required.");

        if (string.IsNullOrWhiteSpace(verificationTokenReference))
            throw new CustomerProfileValidationException("Verification token reference is required.");

        Apply(new CustomerEmailChangeRequested
        {
            CustomerId = Id,
            EncryptedPendingEmail = encryptedPendingEmail,
            VerificationTokenReference = verificationTokenReference,
            RequestedAtUtc = requestedAtUtc,
            ExpiresAtUtc = expiresAtUtc
        });
    }

    public void Forget()
    {
        Apply(new CustomerForgotten(Id));
    }
    
    public void AddAddress(Address address)
    {
        if (_addresses.Any(a => a.Label == address.Label))
            throw new DuplicateAddressLabelException(Id, address.Label);

        Apply(new CustomerAddressAdded(Id, address));
    }

    public void RemoveAddress(string label)
    {
        if (_addresses.Any(x => x.Label == label) == false)
            throw new AddressNotFoundException(Id, label);

        Apply(new CustomerAddressRemoved(Id, label));
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case CustomerCreated e:
                Handle(e);
                break;
            case CustomerProfileUpdated e:
                Handle(e);
                break;
            case CustomerEmailChangeRequested e:
                Handle(e);
                break;
            case CustomerVerifiedEmailChanged e:
                Handle(e);
                break;
            case CustomerForgotten e:
                Handle(e);
                break;
            case CustomerAddressAdded e:
                Handle(e);
                break;
            case CustomerAddressRemoved e:
                Handle(e);
                break;
        }
    }

    private void Handle(CustomerCreated e)
    {
        Id = e.CustomerId;
        EncryptedEmail = e.EncryptedEmail;
        EncryptedFirstName = e.EncryptedFirstName;
        EncryptedLastName = e.EncryptedLastName;
        EncryptedGender = e.EncryptedGender;
        EncryptedDateOfBirth = e.EncryptedDateOfBirth;
    }

    private void Handle(CustomerProfileUpdated e)
    {
        EncryptedFirstName = e.EncryptedFirstName;
        EncryptedLastName = e.EncryptedLastName;
        if (e.EncryptedGender != null)
            EncryptedGender = e.EncryptedGender;
        EncryptedDateOfBirth = e.EncryptedDateOfBirth;
    }

    private void Handle(CustomerEmailChangeRequested e)
    {
        EncryptedPendingEmail = e.EncryptedPendingEmail;
        VerificationTokenReference = e.VerificationTokenReference;
        VerificationExpiresAtUtc = e.ExpiresAtUtc;
    }

    private void Handle(CustomerVerifiedEmailChanged e)
    {
        EncryptedEmail = e.EncryptedVerifiedEmail;
        EncryptedPendingEmail = null;
        VerificationTokenReference = null;
        VerificationExpiresAtUtc = null;
    }

    private void Handle(CustomerForgotten e)
    {
        ForgottenDateUtc = DateTime.UtcNow;
    }

    private void Handle(CustomerAddressAdded e)
    {
        if (e.Address.IsPrimary)
        {
            foreach (var addr in _addresses)
            {
                if (addr.IsPrimary)
                    addr.UnsetPrimary();
            }
        }
        
        _addresses.Add(e.Address);
    }

    private void Handle(CustomerAddressRemoved e)
    {
        var existing = _addresses.FirstOrDefault(x => x.Label == e.Label);
        if (existing != null)
            _addresses.Remove(existing);
    }
}