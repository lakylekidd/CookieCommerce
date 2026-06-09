namespace CookieCommerce.AuthenticationManagement.Application.Keycloak;

public interface IKeycloakAdminService
{
    Task<string> RegisterUserAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default);
    Task<KeycloakUserInfo?> GetUserByEmailAsync(string email, CancellationToken ct = default);
}

public class KeycloakUserInfo
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
