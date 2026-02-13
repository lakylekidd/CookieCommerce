namespace CookieCommerce.CatalogDomain.Domain.DomainEvents;

public class CatalogPriceChanged
{
    public Guid CatalogId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }

    public CatalogPriceChanged(Guid catalogId, decimal oldPrice, decimal newPrice)
    {
        CatalogId = catalogId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
