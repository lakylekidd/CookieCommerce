using CookieCommerce.ProductDomain.Domain.ValueObjects;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Commands;

public sealed record UploadProductMainImageCommand(
    Guid ProductId,
    Stream Content,
    string FileName,
    string ContentType,
    long LengthBytes,
    string? AltText = null) : IRequest<ProductMediaAsset>;

public sealed record RemoveProductMainImageCommand(
    Guid ProductId,
    Guid MediaAssetId) : IRequest;
