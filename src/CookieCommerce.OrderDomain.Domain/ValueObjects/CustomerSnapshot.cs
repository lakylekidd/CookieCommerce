namespace CookieCommerce.OrderDomain.Domain.ValueObjects;

/// <summary>
/// Represents a snapshot of customer information at the time of order placement.
/// Contains only non-PII or masked data for order context.
/// </summary>
public class CustomerSnapshot
{
    /// <summary>
    /// Customer's unique identifier
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// Customer's full name (masked or non-PII version)
    /// </summary>
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>
    /// Masked email address for customer contact reference (e.g., "j***@example.com")
    /// </summary>
    public string MaskedEmail { get; init; } = string.Empty;

    public CustomerSnapshot() { }

    public CustomerSnapshot(Guid customerId, string customerName, string maskedEmail)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        MaskedEmail = maskedEmail;
    }
}
