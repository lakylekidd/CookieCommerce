using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Queries;

/// <summary>
/// Query to retrieve all products.
/// Returns a list of product summaries for browsing.
/// </summary>
public record GetAllProducts() : IRequest<IReadOnlyList<ProductListItemReadModel>>;
