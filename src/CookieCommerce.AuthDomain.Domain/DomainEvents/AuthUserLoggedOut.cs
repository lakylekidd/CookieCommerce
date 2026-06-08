namespace CookieCommerce.AuthDomain.Domain.DomainEvents;

public class AuthUserLoggedOut
{
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = default!;
    public DateTime LoggedOutAt { get; set; }
}
