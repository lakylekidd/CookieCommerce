namespace CookieCommerce.AuthenticationManagement.Domain.DomainEvents;

public class AuthUserRegistered
{
    public Guid UserId { get; set; }
    public Guid? CustomerId { get; set; }
    public string KeycloakId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime RegisteredAt { get; set; }
}
