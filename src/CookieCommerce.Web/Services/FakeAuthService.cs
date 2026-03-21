using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services.StaticData;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CookieCommerce.Web.Services;

public class FakeAuthService(
    ICartService cartService,    
    ProtectedLocalStorage localStorage) : IAuthService
{
    public event Action OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();

    private UserDto? currentUser;
    private bool storageInitialized;
    private string storedKey = "user_email";

    public UserDto? CurrentUser => currentUser;
    public bool IsLoggedIn => currentUser != null;
    public bool IsAdmin => currentUser?.Role == "Admin";

    public async Task InitializeAsync()
    {
        if (storageInitialized)
        {
            return;
        }

        try
        {
            // Look for the "Identity" in the browser's pocket
            var result = await localStorage.GetAsync<string>(storedKey);
            if (result.Success && result.Value != null)
            {
                currentUser = StaticUserList.Users.FirstOrDefault(u => u.Email == result.Value);
                NotifyStateChanged();
            }

            storageInitialized = true;
        }
        catch (InvalidOperationException)
        {
            // Prerendering blocks JS interop; try again after the app becomes interactive.
        }
    }

    public async Task<bool> SignInAsync(string email, string password)
    {
        var user = StaticUserList.Users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user != null)
        {
            currentUser = user;
            await localStorage.SetAsync(storedKey, user.Email);
            await AssignUserToCartAsync();
            NotifyStateChanged();
            return true;
        }
        return false;
    }

    public async Task<AuthResult> SignUpAsync(SignupDto signupForm)
    {
        if (StaticUserList.Users.Any(u => u.Email.Equals(signupForm.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return AuthResult.Failed("User already exists");
        }

        var newUser = new UserDto { FirstName = signupForm.FirstName, LastName = signupForm.LastName, Email = signupForm.Email, Password = signupForm.Password };
        StaticUserList.Users.Add(newUser);
        currentUser = newUser;
        await localStorage.SetAsync("user_email", newUser.Email);
        await AssignUserToCartAsync();
        NotifyStateChanged();
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
        currentUser = null;
        storageInitialized = true;
        localStorage.DeleteAsync("user_email");
        NotifyStateChanged();
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
