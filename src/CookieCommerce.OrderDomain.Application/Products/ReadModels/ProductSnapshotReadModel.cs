namespace CookieCommerce.OrderDomain.Application.Products.ReadModels;

/// <summary>
/// Local read model for product snapshots maintained via integration events.
/// Enables eventual consistency without synchronous HTTP calls to Product domain.
/// </summary>
public class ProductSnapshotReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
}
