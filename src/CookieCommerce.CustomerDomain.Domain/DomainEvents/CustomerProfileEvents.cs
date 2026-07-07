namespace CookieCommerce.CustomerDomain.Domain.DomainEvents;

public class CustomerProfileUpdated
{
    public Guid CustomerId { get; set; }
    public string EncryptedFirstName { get; set; } = string.Empty;
    public string EncryptedLastName { get; set; } = string.Empty;
    public string? EncryptedGender { get; set; }
    public string? EncryptedDateOfBirth { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public class CustomerEmailChangeRequested
{
    public Guid CustomerId { get; set; }
    public string EncryptedPendingEmail { get; set; } = string.Empty;
    public string VerificationTokenReference { get; set; } = string.Empty;
    public DateTimeOffset RequestedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
}

public class CustomerVerifiedEmailChanged
{
    public Guid CustomerId { get; set; }
    public string EncryptedPreviousEmail { get; set; } = string.Empty;
    public string EncryptedVerifiedEmail { get; set; } = string.Empty;
    public DateTimeOffset VerifiedAtUtc { get; set; }
}
