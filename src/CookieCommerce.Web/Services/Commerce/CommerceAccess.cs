namespace CookieCommerce.Web.Services.Commerce;

public sealed class CommerceAccess(IAuthService auth)
{
    public async Task<Guid?> CustomerAsync()
    {
        await auth.InitializeAsync();
        return auth.CurrentUser?.Id;
    }

    public async Task<Guid> RequireCustomerAsync() =>
        await CustomerAsync() ?? throw new UnauthorizedAccessException("Sign in to continue.");

    public async Task RequireAdminAsync()
    {
        await auth.InitializeAsync();
        if (!auth.IsAdmin) throw new UnauthorizedAccessException("Administrator access is required.");
    }
}
