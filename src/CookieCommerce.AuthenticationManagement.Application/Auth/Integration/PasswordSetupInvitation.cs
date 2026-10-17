namespace CookieCommerce.AuthenticationManagement.Application.Auth.Integration;

public sealed class PasswordSetupInvitation
{
    public string Id { get; set; } = string.Empty;
    public string IdentityId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public static string HashToken(string token) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
