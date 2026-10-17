namespace CookieCommerce.AuthenticationManagement.Domain.DomainEvents;

public class AuthUserForgotten
{
    public Guid UserId { get; set; }
    public DateTimeOffset ForgottenAtUtc { get; set; }
    public string Reason { get; set; } = "CustomerForgotten";
}
