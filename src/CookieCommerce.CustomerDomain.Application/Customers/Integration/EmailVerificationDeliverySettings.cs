namespace CookieCommerce.CustomerDomain.Application.Customers.Integration;

/// <summary>
/// Settings governing external email verification delivery handoffs.
/// Configured in Infrastructure and consumed by Application handlers via IOptions&lt;T&gt;.
/// </summary>
public sealed class EmailVerificationDeliverySettings
{
    public const string SectionName = "EmailVerificationDelivery";

    /// <summary>Base URL used to build the absolute verification link sent to the customer.</summary>
    public string VerificationBaseUrl { get; set; } = string.Empty;

    /// <summary>Route path appended to VerificationBaseUrl for the confirmation page.</summary>
    public string VerificationRoute { get; set; } = "/account/email-verification/confirm";

    /// <summary>How long a verification token is valid.</summary>
    public TimeSpan TokenExpiry { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Maximum retries the external delivery dependency should attempt.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Endpoint of the external email delivery dependency.</summary>
    public string ExternalDeliveryEndpoint { get; set; } = string.Empty;
}
