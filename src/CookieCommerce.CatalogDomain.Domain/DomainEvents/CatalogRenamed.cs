namespace CookieCommerce.CatalogDomain.Domain.DomainEvents;

public class CatalogRenamed
{
    public Guid CatalogId { get; }
    public string NewName { get; }

    public CatalogRenamed(Guid catalogId, string newName)
    {
        CatalogId = catalogId;
        NewName = newName;
    }
}
