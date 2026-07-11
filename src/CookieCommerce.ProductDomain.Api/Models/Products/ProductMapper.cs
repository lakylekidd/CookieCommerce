using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.ValueObjects;

namespace CookieCommerce.ProductDomain.Api.Models.Products;

public static class ProductMapper
{
    public static CreateProduct ToCommand(this CreateProductRequest request) =>
        new CreateProduct(
            request.Name,
            request.Description,
            request.Price,
            request.Variants.Select(ToCommand).ToList());

    public static RenameProduct ToRenameCommand(this UpdateProductRequest request, Guid productId) =>
        new RenameProduct(productId, request.Name);

    public static ChangeProductPrice ToChangePriceCommand(this UpdateProductRequest request, Guid productId) =>
        new ChangeProductPrice(productId, request.Price);

    public static UpsertProductVariantsCommand ToVariantCommand(this UpdateProductRequest request, Guid productId) =>
        new(productId, request.Variants.Select(ToCommand).ToList());

    public static UpsertProductVariantsCommand ToVariantCommand(this IReadOnlyCollection<ProductVariantRequest> request, Guid productId) =>
        new(productId, request.Select(ToCommand).ToList());

    private static UpsertProductVariant ToCommand(this ProductVariantRequest request) =>
        new(request.Id, request.Name, request.Price, request.CurrencySymbol, request.IsActive, request.Sku);

    public static ProductDetailsResponse ToResponse(this ProductDetailsReadModel readModel) => new(
        readModel.Id,
        readModel.Name,
        readModel.Description,
        readModel.Price,
        readModel.IsActive,
        readModel.MainImage?.ToResponse(),
        readModel.Variants.Select(ToResponse).ToList());

    public static ProductListItemResponse ToResponse(this ProductListItemReadModel readModel) => new(
        readModel.Id,
        readModel.Name,
        readModel.Price,
        readModel.IsActive,
        readModel.MainImage?.ToResponse(),
        readModel.Variants.Select(ToResponse).ToList());

    private static ProductVariantResponse ToResponse(this ProductVariantReadModel readModel) => new(
        readModel.Id,
        readModel.Name,
        readModel.Sku,
        readModel.Price,
        readModel.CurrencySymbol,
        readModel.IsActive,
        readModel.Photos.Select(ToResponse).ToList());

    public static ProductMediaAssetResponse ToResponse(this ProductMediaAssetReadModel readModel) => new(
        readModel.Id,
        readModel.ProductId,
        readModel.VariantId,
        readModel.Role,
        readModel.Url,
        readModel.OriginalFileName,
        readModel.ContentType,
        readModel.LengthBytes,
        readModel.AltText);

    public static ProductMediaAssetResponse ToResponse(this ProductMediaAsset mediaAsset) => new(
        mediaAsset.Id,
        mediaAsset.ProductId,
        mediaAsset.VariantId,
        mediaAsset.Role.ToString(),
        mediaAsset.Url,
        mediaAsset.OriginalFileName,
        mediaAsset.ContentType,
        mediaAsset.LengthBytes,
        mediaAsset.AltText);
}
