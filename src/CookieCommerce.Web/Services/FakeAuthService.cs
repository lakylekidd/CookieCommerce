using CookieCommerce.Web.Models.Auth;

namespace CookieCommerce.Web.Services;

public class FakeAuthService : IAuthService
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
            return true;
        }
        return false;
    }

    public async Task<bool> SignUpAsync(string firstName, string lastName, string email, string password)
    {
        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return false; // User already exists
        }

        var newUser = new UserDto { FirstName = firstName, LastName = lastName, Email = email, Password = password };
        _users.Add(newUser);
        CurrentUser = newUser; // Auto-login after signup
        return true;
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}
