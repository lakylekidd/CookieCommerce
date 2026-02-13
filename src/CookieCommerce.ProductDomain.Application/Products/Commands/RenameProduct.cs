using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Commands;

/// <summary>
/// Command to rename an existing product.
/// </summary>
/// <param name="ProductId">The ID of the product to rename</param>
/// <param name="NewName">The new name for the product</param>
public record RenameProduct(
    Guid ProductId,
    string NewName) : IRequest;
