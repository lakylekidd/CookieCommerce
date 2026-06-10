namespace CookieCommerce.AuthenticationManagement.Api.Models;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = default!;
}
