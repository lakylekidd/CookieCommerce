using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Commands;

public sealed record ActivateProduct(Guid ProductId) : IRequest;
