using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using Marten.Events.Aggregation;

namespace CookieCommerce.ProductDomain.Infrastructure.Projections;

/// <summary>
/// Projection for building the ProductListItemReadModel from product domain events.
/// This projection maintains a lightweight view of products for list/browse scenarios.
/// </summary>
public class ProductListItemProjection : SingleStreamProjection<ProductListItemReadModel, Guid>
{
    /// <summary>
    /// Creates the initial read model from the ProductCreated event.
    /// </summary>
    public ProductListItemReadModel Create(ProductCreated @event)
    {
        return new ProductListItemReadModel
        {
            Id = @event.ProductId,
            Name = @event.Name,
            Price = @event.Price,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the product name when a ProductRenamed event occurs.
    /// </summary>
    public ProductListItemReadModel Apply(ProductRenamed @event, ProductListItemReadModel current)
    {
        current.Name = @event.NewName;
        return current;
    }

    /// <summary>
    /// Updates the product price when a ProductPriceChanged event occurs.
    /// </summary>
    public ProductListItemReadModel Apply(ProductPriceChanged @event, ProductListItemReadModel current)
    {
        current.Price = @event.NewPrice;
        return current;
    }

    /// <summary>
    /// Marks the product as inactive when a ProductDeactivated event occurs.
    /// </summary>
    public ProductListItemReadModel Apply(ProductDeactivated @event, ProductListItemReadModel current)
    {
        current.IsActive = false;
        return current;
    }

    public ProductListItemReadModel Apply(ProductVariantAdded @event, ProductListItemReadModel current)
    {
        current.Variants.RemoveAll(variant => variant.Id == @event.VariantId);
        current.Variants.Add(new ProductVariantReadModel
        {
            Id = @event.VariantId,
            Name = @event.Name,
            Sku = @event.Sku,
            Price = @event.Price,
            CurrencySymbol = @event.CurrencySymbol,
            IsActive = @event.IsActive
        });

        return current;
    }

    public ProductListItemReadModel Apply(ProductVariantUpdated @event, ProductListItemReadModel current)
    {
        var variant = current.Variants.FirstOrDefault(candidate => candidate.Id == @event.VariantId);
        if (variant is not null)
        {
            variant.Name = @event.Name;
            variant.Sku = @event.Sku;
            variant.Price = @event.Price;
            variant.CurrencySymbol = @event.CurrencySymbol;
            variant.IsActive = @event.IsActive;
        }

        return current;
    }

    public ProductListItemReadModel Apply(ProductVariantRemoved @event, ProductListItemReadModel current)
    {
        current.Variants.RemoveAll(variant => variant.Id == @event.VariantId);
        return current;
    }

    public ProductListItemReadModel Apply(ProductMainImageSet @event, ProductListItemReadModel current)
    {
        current.MainImage = ProductProjectionMediaMapper.ToReadModel(@event.MediaAsset);
        return current;
    }

    public ProductListItemReadModel Apply(ProductMainImageRemoved @event, ProductListItemReadModel current)
    {
        if (current.MainImage?.Id == @event.MediaAssetId)
            current.MainImage = null;

        return current;
    }

    public ProductListItemReadModel Apply(VariantPhotoAdded @event, ProductListItemReadModel current)
    {
        var variant = current.Variants.FirstOrDefault(candidate => candidate.Id == @event.VariantId);
        if (variant is not null)
        {
            variant.Photos.RemoveAll(photo => photo.Id == @event.MediaAsset.Id);
            variant.Photos.Add(ProductProjectionMediaMapper.ToReadModel(@event.MediaAsset));
        }

        return current;
    }

    public ProductListItemReadModel Apply(VariantPhotoRemoved @event, ProductListItemReadModel current)
    {
        var variant = current.Variants.FirstOrDefault(candidate => candidate.Id == @event.VariantId);
        variant?.Photos.RemoveAll(photo => photo.Id == @event.MediaAssetId);
        return current;
    }
}
