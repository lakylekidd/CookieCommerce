using CookieCommerce.ProductDomain.Domain.ValueObjects;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Commands;

public sealed record UploadVariantPhotoCommand(
    Guid ProductId,
    Guid VariantId,
    Stream Content,
    string FileName,
    string ContentType,
    long LengthBytes,
    string? AltText = null,
    int? SortOrder = null) : IRequest<ProductMediaAsset>;

public sealed record RemoveVariantPhotoCommand(
    Guid ProductId,
    Guid VariantId,
    Guid MediaAssetId) : IRequest;
