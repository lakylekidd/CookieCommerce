namespace CookieCommerce.AuthenticationManagement.Api.Models;

public class RegisterRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = default!;
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(8)]
    public string Password { get; set; } = default!;
    [System.ComponentModel.DataAnnotations.Required]
    public string FirstName { get; set; } = default!;
    [System.ComponentModel.DataAnnotations.Required]
    public string LastName { get; set; } = default!;
}
