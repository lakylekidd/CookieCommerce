namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

public sealed class ProductionAdminOptions
{
    public bool Enabled { get; set; }
    public string Email { get; set; } = "";
    public string InitialPassword { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
