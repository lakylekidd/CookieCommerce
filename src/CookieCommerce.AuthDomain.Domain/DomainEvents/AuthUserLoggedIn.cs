namespace CookieCommerce.AuthDomain.Domain.DomainEvents;

public class AuthUserLoggedIn
{
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = default!;
    public DateTime LoggedInAt { get; set; }
}
