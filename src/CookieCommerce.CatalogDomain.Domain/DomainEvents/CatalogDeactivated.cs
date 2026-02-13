namespace CookieCommerce.CatalogDomain.Domain.DomainEvents;

public class CatalogDeactivated
{
    public Guid CatalogId { get; }

    public CatalogDeactivated(Guid catalogId)
    {
        CatalogId = catalogId;
    }
}
