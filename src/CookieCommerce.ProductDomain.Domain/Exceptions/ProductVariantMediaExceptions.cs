namespace CookieCommerce.ProductDomain.Domain.Exceptions;

public sealed class DuplicateProductVariantException(Guid productId, string variantName)
    : Exception($"Product '{productId}' already has a variant named '{variantName}'.")
{
    public Guid ProductId { get; } = productId;
    public string VariantName { get; } = variantName;
}

public sealed class InvalidProductVariantException(string message)
    : Exception(message);

public sealed class InvalidProductMediaOwnershipException(Guid productId, Guid mediaAssetId, string reason)
    : Exception($"Media asset '{mediaAssetId}' is not valid for product '{productId}': {reason}.")
{
    public Guid ProductId { get; } = productId;
    public Guid MediaAssetId { get; } = mediaAssetId;
    public string Reason { get; } = reason;
}

public sealed class ProductMainImageCardinalityException(Guid productId)
    : Exception($"Product '{productId}' can have at most one current main image.")
{
    public Guid ProductId { get; } = productId;
}
