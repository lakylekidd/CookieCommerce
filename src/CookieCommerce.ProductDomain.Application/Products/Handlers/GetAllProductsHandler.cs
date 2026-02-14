using CookieCommerce.ProductDomain.Application.Products.Queries;
using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

/// <summary>
/// Handles the query to retrieve all products.
/// Queries the read model store to return a list of product summaries.
/// </summary>
public class GetAllProductsHandler(IQuerySession query) : IRequestHandler<GetAllProducts, IReadOnlyList<ProductListItemReadModel>>
{
    private readonly IQuerySession _query = query;

    public async Task<IReadOnlyList<ProductListItemReadModel>> Handle(
        GetAllProducts request,
        CancellationToken cancellationToken)
    {
        return await _query.Query<ProductListItemReadModel>().ToListAsync(cancellationToken);
    }
}
