namespace CookieCommerce.CatalogDomain.Domain.DomainEvents;

public class CatalogCreated
{
    public Guid CatalogId { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public decimal Price { get; init; }
}
