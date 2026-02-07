using CookieCommerce.CustomerDomain.Domain.DomainEvents;
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

    public void Forget()
    {
        Apply(new CustomerForgotten(Id));
    }
    
    public void AddAddress(Address address)
    {
        if (_addresses.Any(a => a.Label == address.Label))
            throw new InvalidOperationException($"An address with the label '{address.Label}' already exists for customer ."); // TODO: Better exception and logging

        Apply(new CustomerAddressAdded(Id, address));
    }

    public void RemoveAddress(string label)
    {
        if (_addresses.Any(x => x.Label == label) == false)
            throw new InvalidOperationException($"Address label '{label}' does not exist.");

        Apply(new CustomerAddressRemoved(Id, label));
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case CustomerCreated e:
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