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
    Task SendPasswordResetAsync(string keycloakId, CancellationToken ct = default);
    Task<KeycloakUserInfo?> GetUserByIdAsync(string keycloakId, CancellationToken ct = default);
    Task<KeycloakAccountStatus> GetAccountStatusAsync(string keycloakId, CancellationToken ct = default);
    Task ClearLoginFailuresAsync(string keycloakId, CancellationToken ct = default);
    Task SetPasswordAsync(string keycloakId, string password, CancellationToken ct = default);
    Task EnableUserAsync(string keycloakId, CancellationToken ct = default);
    Task ConfigureEmailAsync(KeycloakEmailSettings settings, CancellationToken ct = default);
    Task<string> ProvisionCustomerAsync(Guid customerId, string email, string firstName, string lastName, CancellationToken ct = default);
    Task<KeycloakUserInfo?> GetUserByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
}

public sealed record KeycloakAccountStatus(string? IdentityId, string State, int FailedAttempts = 0, string? LastRecoveryState = null);
public sealed record KeycloakEmailSettings(string Host, int Port, string Security, string Username,
    string Password, string From, string SenderName, string ReplyTo, string PublicStoreUrl);

public class KeycloakUserInfo
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public Guid? CustomerId { get; set; }
}
