namespace CookieCommerce.OrderDomain.Application.Customers;

/// <summary>
/// Helper methods for customer-related operations.
/// </summary>
public static class CustomerHelpers
{
    /// <summary>
    /// Masks an email address for privacy protection.
    /// Example: "john.doe@example.com" becomes "j***@example.com"
    /// </summary>
    /// <param name="email">The email address to mask</param>
    /// <returns>Masked email address</returns>
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return "***";

        var localPart = email[..atIndex];
        var domain = email[atIndex..];

        if (localPart.Length <= 1)
            return $"{localPart}***{domain}";

        return $"{localPart[0]}***{domain}";
    }
}
