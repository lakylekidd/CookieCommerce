namespace CookieCommerce.AuthenticationManagement.Application.Keycloak;

public interface IKeycloakAdminService
{
    Task<string> RegisterUserAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default);
    Task AssignRealmRoleAsync(string keycloakId, string roleName, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRealmRolesAsync(string keycloakId, CancellationToken ct = default);
    Task<KeycloakUserInfo?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task UpdateUserEmailAsync(string keycloakId, string verifiedEmail, CancellationToken ct = default);
    Task UpdateUserProfileAsync(string keycloakId, string firstName, string lastName, string? gender, DateOnly? dateOfBirth, CancellationToken ct = default);
    Task DisableUserAsync(string keycloakId, CancellationToken ct = default);
}

public class KeycloakUserInfo
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
