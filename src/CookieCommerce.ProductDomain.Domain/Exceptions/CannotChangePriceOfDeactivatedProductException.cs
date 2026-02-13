namespace CookieCommerce.ProductDomain.Domain.Exceptions;

public class CannotChangePriceOfDeactivatedProductException : Exception
{
    public Guid ProductId { get; }
    public decimal NewPrice { get; }

    public CannotChangePriceOfDeactivatedProductException(Guid productId, decimal newPrice) 
        : base($"Cannot change price of deactivated product {productId} to {newPrice}.")
    {
        ProductId = productId;
        NewPrice = newPrice;
    }
}
