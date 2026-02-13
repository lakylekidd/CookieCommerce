namespace CookieCommerce.CatalogDomain.Domain.Exceptions;

public class InvalidPriceException : Exception
{
    public Guid CatalogId { get; }
    public decimal Price { get; }

    public InvalidPriceException(Guid catalogId, decimal price) 
        : base($"Invalid price for catalog item {catalogId}: {price}. Price must be greater than 0.")
    {
        CatalogId = catalogId;
        Price = price;
    }
}
