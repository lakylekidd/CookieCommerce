namespace CookieCommerce.AuthenticationManagement.Domain.DomainEvents;

public class AuthUserEmailChanged
{
    public Guid UserId { get; set; }
    public string PreviousEmail { get; set; } = default!;
    public string VerifiedEmail { get; set; } = default!;
    public DateTimeOffset ChangedAtUtc { get; set; }
}
