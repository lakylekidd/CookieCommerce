using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Services;

public class FakeAuthService(ICartService cartService) : IAuthService
{
    // Static list to keep users in memory during the session
    private static readonly List<UserDto> _users =
    [
        new UserDto { Email = "tiger@cookie.com", Password = "Password123", FirstName = "Tiger", LastName = "van Cookie" }
    ];

    public UserDto? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public async Task<bool> SignInAsync(string email, string password)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user != null)
        {
            CurrentUser = user;
            await AssignUserToCartAsync();
            return true;
        }
        return false;
    }

    public async Task<AuthResult> SignUpAsync(SignupDto signupForm)
    {
        if (_users.Any(u => u.Email.Equals(signupForm.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return AuthResult.Failed("User already exists");
        }

        var newUser = new UserDto { FirstName = signupForm.FirstName, LastName = signupForm.LastName, Email = signupForm.Email, Password = signupForm.Password };
        _users.Add(newUser);
        CurrentUser = newUser;
        return AuthResult.Succeeded();
    }

    public Task SendForgotPasswordLinkAsync(string email)
    {
        // In a real implementation, you would send an email with a reset link.
        // For this fake service, we'll just simulate the delay.
        return Task.Delay(3000);
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    private async Task AssignUserToCartAsync()
    {
        if (CurrentUser == null) return;
        await cartService.ConvertToUserCartAsync(CurrentUser.Id);
    }

    public Task<AuthResult> UpdatePasswordAsync(string token, string newPassword)
    {
        // In a real implementation, you would verify the token and update the password.
        // For this fake service, we'll just simulate success.
        return Task.FromResult(AuthResult.Succeeded());
    }
}
