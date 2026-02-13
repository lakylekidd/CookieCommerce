using MediatR;

namespace CookieCommerce.CatalogDomain.Application.Products.Commands;

/// <summary>
/// Command to change the price of an existing product.
/// </summary>
/// <param name="ProductId">The ID of the product</param>
/// <param name="NewPrice">The new price (must be greater than 0)</param>
public record ChangeProductPrice(
    Guid ProductId,
    decimal NewPrice) : IRequest;
