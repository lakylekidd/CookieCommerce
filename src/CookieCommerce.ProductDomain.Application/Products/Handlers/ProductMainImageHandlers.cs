using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Media;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

public sealed class UploadProductMainImageHandler(
    IDocumentSession session,
    IProductMediaStore mediaStore,
    IProductImageValidator imageValidator) : IRequestHandler<UploadProductMainImageCommand, ProductMediaAsset>
{
    public async Task<ProductMediaAsset> Handle(UploadProductMainImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Content is null)
            throw new InvalidProductVariantException("An image file is required.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new InvalidProductVariantException("Image file name is required.");

        if (string.IsNullOrWhiteSpace(request.ContentType))
            throw new InvalidProductVariantException("Image content type is required.");

        if (request.Content.CanSeek)
            request.Content.Position = 0;

        var validation = await imageValidator.ValidateAsync(request.Content, request.ContentType, request.LengthBytes, cancellationToken);
        if (!validation.IsValid)
            throw new InvalidProductVariantException(validation.ErrorMessage ?? "The uploaded image is invalid.");

        if (request.Content.CanSeek)
            request.Content.Position = 0;

        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId,
            token: cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        var previousMainImage = product.MainImage;
        var writeResult = await mediaStore.SaveAsync(new ProductMediaStoreWriteRequest(
            request.ProductId,
            null,
            ProductMediaRole.MainImage,
            request.FileName,
            request.ContentType,
            request.Content), cancellationToken);

        var mediaAsset = new ProductMediaAsset(
            Guid.CreateVersion7(),
            request.ProductId,
            null,
            ProductMediaRole.MainImage,
            writeResult.StorageKey,
            writeResult.Url,
            request.FileName,
            request.ContentType,
            writeResult.LengthBytes,
            writeResult.Sha256,
            request.AltText,
            DateTimeOffset.UtcNow);

        product.SetMainImage(mediaAsset);
        session.Events.Append(request.ProductId, product.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        if (previousMainImage is not null)
            await mediaStore.DeleteAsync(previousMainImage.StorageKey, cancellationToken);

        return mediaAsset;
    }
}

public sealed class RemoveProductMainImageHandler(
    IDocumentSession session,
    IProductMediaStore mediaStore) : IRequestHandler<RemoveProductMainImageCommand>
{
    public async Task Handle(RemoveProductMainImageCommand request, CancellationToken cancellationToken)
    {
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId,
            token: cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        var mediaAsset = product.MainImage?.Id == request.MediaAssetId ? product.MainImage : null;
        product.RemoveMainImage(request.MediaAssetId);

        if (product.DomainEvents.Count > 0)
        {
            session.Events.Append(request.ProductId, product.DomainEvents);
            await session.SaveChangesAsync(cancellationToken);
        }

        if (mediaAsset is not null)
            await mediaStore.DeleteAsync(mediaAsset.StorageKey, cancellationToken);
    }
}
