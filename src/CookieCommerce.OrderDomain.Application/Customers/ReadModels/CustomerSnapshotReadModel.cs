namespace CookieCommerce.OrderDomain.Application.Customers.ReadModels;

/// <summary>
/// Replicated customer snapshot in Order domain for eventual consistency.
/// Populated from CustomerSnapshotPublished integration events.
/// </summary>
public class CustomerSnapshotReadModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public DateTime LastUpdated { get; set; }
}
