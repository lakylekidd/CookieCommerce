namespace CookieCommerce.AuthenticationManagement.Application.Auth.Integration;

public sealed class PasswordRecoveryDelivery
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string IdentityId { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(30);
    public DateTimeOffset RetryAfter { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string State { get; set; } = "Queued";
    public int Attempts { get; set; }
}
