using System.ComponentModel.DataAnnotations;

namespace CookieCommerce.Web.Models.Auth;

public class SignupDto
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = "";

    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = "";

    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = "";

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms")]
    public bool AgreeToTerms { get; set; }

    public bool SubscribeNewsletter { get; set; }
}
