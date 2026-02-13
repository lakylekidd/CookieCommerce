namespace CookieCommerce.CatalogDomain.Domain.Exceptions;

public class CannotChangePriceOfDeactivatedCatalogException : Exception
{
    public Guid CatalogId { get; }
    public decimal NewPrice { get; }

    public CannotChangePriceOfDeactivatedCatalogException(Guid catalogId, decimal newPrice) 
        : base($"Cannot change price of deactivated catalog item {catalogId} to {newPrice}.")
    {
        CatalogId = catalogId;
        NewPrice = newPrice;
    }
}
