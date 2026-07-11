using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Commands;

public sealed record UpsertProductVariant(
    Guid? Id,
    string Name,
    decimal Price,
    string CurrencySymbol = "€",
    bool IsActive = true,
    string? Sku = null);

public sealed record UpsertProductVariantsCommand(
    Guid ProductId,
    IReadOnlyCollection<UpsertProductVariant> Variants) : IRequest;
