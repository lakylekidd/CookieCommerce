// This file is kept for backward compatibility. The canonical settings type is now
// EmailVerificationDeliverySettings in the Application layer so that Application
// handlers can consume it without referencing Infrastructure.
// Infrastructure DI binds EmailVerificationDeliverySettings from the same config section.

namespace CookieCommerce.CustomerDomain.Infrastructure.Configuration;

/// <summary>
/// Legacy options class — superseded by EmailVerificationDeliverySettings in Application layer.
/// Retained for any direct Infrastructure-only config needs.
/// </summary>
[Obsolete("Use EmailVerificationDeliverySettings from the Application layer instead.")]
public sealed class EmailVerificationDeliveryOptions
{
    public const string SectionName = "EmailVerificationDelivery";

    public string VerificationBaseUrl { get; set; } = string.Empty;
    public string VerificationRoute { get; set; } = "/account/email-verification/confirm";
    public TimeSpan TokenExpiry { get; set; } = TimeSpan.FromHours(24);
    public int MaxRetryAttempts { get; set; } = 3;
    public string ExternalDeliveryEndpoint { get; set; } = string.Empty;
}
