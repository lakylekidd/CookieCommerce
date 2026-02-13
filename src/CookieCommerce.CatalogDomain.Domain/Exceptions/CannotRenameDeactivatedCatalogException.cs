namespace CookieCommerce.CatalogDomain.Domain.Exceptions;

public class CannotRenameDeactivatedCatalogException : Exception
{
    public Guid CatalogId { get; }
    public string NewName { get; }

    public CannotRenameDeactivatedCatalogException(Guid catalogId, string newName) 
        : base($"Cannot rename deactivated catalog item {catalogId} to '{newName}'.")
    {
        CatalogId = catalogId;
        NewName = newName;
    }
}
