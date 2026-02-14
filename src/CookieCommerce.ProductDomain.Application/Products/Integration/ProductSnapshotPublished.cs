namespace CookieCommerce.ProductDomain.Application.Products.Integration;

/// <summary>
/// Integration event published when a product snapshot is created or updated.
/// This provides other services with the current state of a product.
/// </summary>
public record ProductSnapshotPublished(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    bool IsActive);
