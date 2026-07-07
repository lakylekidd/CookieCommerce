namespace CookieCommerce.ProductDomain.Domain.Exceptions;

public class CannotRenameDeactivatedProductException : Exception
{
    public Guid ProductId { get; }
    public string NewName { get; }

    public CannotRenameDeactivatedProductException(Guid productId, string newName)
        : base($"Cannot rename deactivated product {productId} to '{newName}'.")
    {
        ProductId = productId;
        NewName = newName;
    }
}
