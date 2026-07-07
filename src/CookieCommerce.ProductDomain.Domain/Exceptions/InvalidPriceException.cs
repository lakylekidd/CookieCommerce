namespace CookieCommerce.ProductDomain.Domain.Exceptions;

public class InvalidPriceException : Exception
{
    public Guid ProductId { get; }
    public decimal Price { get; }

    public InvalidPriceException(Guid productId, decimal price)
        : base($"Invalid price for product {productId}: {price}. Price must be greater than 0.")
    {
        ProductId = productId;
        Price = price;
    }
}
