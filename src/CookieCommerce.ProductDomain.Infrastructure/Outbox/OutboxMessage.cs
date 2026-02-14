namespace CookieCommerce.ProductDomain.Infrastructure.Outbox;

/// <summary>
/// Represents an outbox message for transactional publishing of integration events.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool IsProcessed { get; set; }
    public string? Error { get; set; }
}
