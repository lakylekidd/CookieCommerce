using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Queries;

/// <summary>
/// Query to retrieve a product by its unique identifier.
/// Returns full product details if found, otherwise null.
/// </summary>
public record GetProductById(Guid ProductId) : IRequest<ProductDetailsReadModel?>;
