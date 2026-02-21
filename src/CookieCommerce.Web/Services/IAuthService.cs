using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Services;

public interface IAuthService
{
    public bool IsLoggedIn { get; }
    public UserDto? CurrentUser { get; }
    public Task<bool> SignInAsync(string email, string password);
    public Task<AuthResult> SignUpAsync(SignupDto signupForm);
    public void Logout();
}
