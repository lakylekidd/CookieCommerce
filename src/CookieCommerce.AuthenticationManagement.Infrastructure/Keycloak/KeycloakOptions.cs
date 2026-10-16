namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

public class KeycloakOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public string Realm { get; set; } = "cookiecommerce";
    public string ClientId { get; set; } = "cookiecommerce-backend";
    public string ClientSecret { get; set; } = "cookiecommerce-secret";
    public string AdminUser { get; set; } = "admin";
    public string AdminPassword { get; set; } = "admin";
    public string PublicStoreUrl { get; set; } = "";
    public string AdminClientId { get; set; } = "cookiecommerce-account-admin";
    public string AdminClientSecret { get; set; } = "cookiecommerce-development-admin-secret";
}
