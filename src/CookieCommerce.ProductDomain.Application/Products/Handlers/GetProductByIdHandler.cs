using CookieCommerce.ProductDomain.Application.Products.Queries;
using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handles the query to retrieve a product by its unique identifier.
/// Returns full product details if found, otherwise null.
/// </summary>
public class GetProductByIdHandler(IQuerySession query) : IRequestHandler<GetProductById, ProductDetailsReadModel?>
{
    private readonly IQuerySession _query = query;

    public async Task<ProductDetailsReadModel?> Handle(
        GetProductById request,
        CancellationToken cancellationToken) => await _query.LoadAsync<ProductDetailsReadModel>(request.ProductId, cancellationToken);
}
