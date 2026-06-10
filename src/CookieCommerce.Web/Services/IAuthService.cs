using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Services;

public interface IAuthService
{
    public bool IsLoggedIn { get; }
    public bool IsAdmin { get; }
    public UserDto? CurrentUser { get; }
    public Task<bool> SignInAsync(string email, string password);
    public Task<AuthResult> SignUpAsync(SignupDto signupForm);
    public Task SendForgotPasswordLinkAsync(string email);
    public Task<AuthResult> UpdatePasswordAsync(string token, string newPassword);
    public Task InitializeAsync();
    public void Logout();
    event Action OnChange;
}
