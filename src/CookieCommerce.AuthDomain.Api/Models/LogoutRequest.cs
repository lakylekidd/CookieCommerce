namespace CookieCommerce.AuthDomain.Api.Models;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = default!;
}
