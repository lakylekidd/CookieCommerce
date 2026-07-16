using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Commands;

/// <summary>
/// Command to create a new product in the catalog.
/// </summary>
/// <param name="Name">The name of the product</param>
/// <param name="Description">The description of the product</param>
/// <param name="Price">The price of the product (must be greater than 0)</param>
public record CreateProduct(
    string Name,
    string Description,
    decimal Price,
    IReadOnlyCollection<UpsertProductVariant>? Variants = null) : IRequest<Guid>;
