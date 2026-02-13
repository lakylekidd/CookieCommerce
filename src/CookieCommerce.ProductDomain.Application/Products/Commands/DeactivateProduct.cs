using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Commands;

/// <summary>
/// Command to deactivate a product, preventing further modifications.
/// </summary>
/// <param name="ProductId">The ID of the product to deactivate</param>
public record DeactivateProduct(
    Guid ProductId) : IRequest;
