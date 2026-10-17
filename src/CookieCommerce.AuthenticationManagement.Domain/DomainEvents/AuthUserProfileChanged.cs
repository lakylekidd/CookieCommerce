namespace CookieCommerce.AuthenticationManagement.Domain.DomainEvents;

public sealed class AuthUserProfileChanged
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
}
