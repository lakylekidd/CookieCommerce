using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Media;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using Marten;
using MediatR;

namespace CookieCommerce.ProductDomain.Application.Products.Handlers;

public sealed class UploadVariantPhotoHandler(
    IDocumentSession session,
    IProductMediaStore mediaStore,
    IProductImageValidator imageValidator) : IRequestHandler<UploadVariantPhotoCommand, ProductMediaAsset>
{
    public async Task<ProductMediaAsset> Handle(UploadVariantPhotoCommand request, CancellationToken cancellationToken)
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

        if (product.Variants.All(variant => variant.Id != request.VariantId))
            throw new InvalidProductMediaOwnershipException(request.ProductId, Guid.Empty, $"variant '{request.VariantId}' does not belong to the product");

        var writeResult = await mediaStore.SaveAsync(new ProductMediaStoreWriteRequest(
            request.ProductId,
            request.VariantId,
            ProductMediaRole.VariantPhoto,
            request.FileName,
            request.ContentType,
            request.Content), cancellationToken);

        var mediaAsset = new ProductMediaAsset(
            Guid.CreateVersion7(),
            request.ProductId,
            request.VariantId,
            ProductMediaRole.VariantPhoto,
            writeResult.StorageKey,
            writeResult.Url,
            request.FileName,
            request.ContentType,
            writeResult.LengthBytes,
            writeResult.Sha256,
            request.AltText,
            DateTimeOffset.UtcNow);

        product.AddVariantPhoto(request.VariantId, mediaAsset);
        session.Events.Append(request.ProductId, product.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        return mediaAsset;
    }
}

public sealed class RemoveVariantPhotoHandler(
    IDocumentSession session,
    IProductMediaStore mediaStore) : IRequestHandler<RemoveVariantPhotoCommand>
{
    public async Task Handle(RemoveVariantPhotoCommand request, CancellationToken cancellationToken)
    {
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(
            request.ProductId,
            token: cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        var variant = product.Variants.FirstOrDefault(candidate => candidate.Id == request.VariantId);
        if (variant is null)
            throw new InvalidProductMediaOwnershipException(request.ProductId, request.MediaAssetId, $"variant '{request.VariantId}' does not belong to the product");

        var mediaAsset = variant.Photos.FirstOrDefault(photo => photo.Id == request.MediaAssetId);
        product.RemoveVariantPhoto(request.VariantId, request.MediaAssetId);

        if (product.DomainEvents.Count > 0)
        {
            session.Events.Append(request.ProductId, product.DomainEvents);
            await session.SaveChangesAsync(cancellationToken);
        }

        if (mediaAsset is not null)
            await mediaStore.DeleteAsync(mediaAsset.StorageKey, cancellationToken);
    }
}
