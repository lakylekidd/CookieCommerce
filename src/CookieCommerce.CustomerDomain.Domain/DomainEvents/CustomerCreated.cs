namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerCreated
{
    public Guid CustomerId { get; set; }
    public string EncryptedFirstName { get; set; }
    public string EncryptedLastName { get; set; }
    public string EncryptedGender { get; set; }
    public string EncryptedEmail { get; set; }
    public string? EncryptedDateOfBirth { get; set; }
}