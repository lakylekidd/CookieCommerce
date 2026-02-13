namespace CookieCommerce.ProductDomain.Domain.DomainEvents;

public class ProductCreated
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public decimal Price { get; init; }
}
