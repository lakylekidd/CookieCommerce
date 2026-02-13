namespace CookieCommerce.ProductDomain.Domain.DomainEvents;

public class ProductCreated
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}
