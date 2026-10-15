namespace CookieCommerce.AuthenticationManagement.Api.Models;

public class LoginRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = default!;
    [System.ComponentModel.DataAnnotations.Required]
    public string Password { get; set; } = default!;
}
