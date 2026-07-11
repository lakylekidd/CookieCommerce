namespace CookieCommerce.ProductDomain.Application.Products.Media;

public sealed record ProductImageValidationResult(bool IsValid, string? ErrorMessage = null)
{
    public static ProductImageValidationResult Success { get; } = new(true);
    public static ProductImageValidationResult Failure(string errorMessage) => new(false, errorMessage);
}

public interface IProductImageValidator
{
    Task<ProductImageValidationResult> ValidateAsync(
        Stream imageStream,
        string contentType,
        long lengthBytes,
        CancellationToken cancellationToken);
}
