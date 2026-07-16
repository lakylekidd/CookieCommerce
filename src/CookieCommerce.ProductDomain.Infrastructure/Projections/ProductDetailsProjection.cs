using CookieCommerce.ProductDomain.Application.Products.ReadModels;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using Marten.Events.Aggregation;

namespace CookieCommerce.ProductDomain.Infrastructure.Projections;

/// <summary>
/// Projection for building the ProductDetailsReadModel from product domain events.
/// This projection maintains a complete view of products including full descriptions.
/// </summary>
public class ProductDetailsProjection : SingleStreamProjection<ProductDetailsReadModel, Guid>
{
    /// <summary>
    /// Creates the initial read model from the ProductCreated event.
    /// </summary>
    public ProductDetailsReadModel Create(ProductCreated @event)
    {
        return new ProductDetailsReadModel
        {
            Id = @event.ProductId,
            Name = @event.Name,
            Description = @event.Description,
            Price = @event.Price,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the product name when a ProductRenamed event occurs.
    /// </summary>
    public ProductDetailsReadModel Apply(ProductRenamed @event, ProductDetailsReadModel current)
    {
        current.Name = @event.NewName;
        return current;
    }

    /// <summary>
    /// Updates the product price when a ProductPriceChanged event occurs.
    /// </summary>
    public ProductDetailsReadModel Apply(ProductPriceChanged @event, ProductDetailsReadModel current)
    {
        current.Price = @event.NewPrice;
        return current;
    }

    /// <summary>
    /// Marks the product as inactive when a ProductDeactivated event occurs.
    /// </summary>
    public ProductDetailsReadModel Apply(ProductDeactivated @event, ProductDetailsReadModel current)
    {
        current.IsActive = false;
        return current;
    }

    public ProductDetailsReadModel Apply(ProductVariantAdded @event, ProductDetailsReadModel current)
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

    public ProductDetailsReadModel Apply(ProductVariantUpdated @event, ProductDetailsReadModel current)
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

    public ProductDetailsReadModel Apply(ProductVariantRemoved @event, ProductDetailsReadModel current)
    {
        current.Variants.RemoveAll(variant => variant.Id == @event.VariantId);
        return current;
    }

    public ProductDetailsReadModel Apply(ProductMainImageSet @event, ProductDetailsReadModel current)
    {
        current.MainImage = ProductProjectionMediaMapper.ToReadModel(@event.MediaAsset);
        return current;
    }

    public ProductDetailsReadModel Apply(ProductMainImageRemoved @event, ProductDetailsReadModel current)
    {
        if (current.MainImage?.Id == @event.MediaAssetId)
            current.MainImage = null;

        return current;
    }

    public ProductDetailsReadModel Apply(VariantPhotoAdded @event, ProductDetailsReadModel current)
    {
        var variant = current.Variants.FirstOrDefault(candidate => candidate.Id == @event.VariantId);
        if (variant is not null)
        {
            variant.Photos.RemoveAll(photo => photo.Id == @event.MediaAsset.Id);
            variant.Photos.Add(ProductProjectionMediaMapper.ToReadModel(@event.MediaAsset));
        }

        return current;
    }

    public ProductDetailsReadModel Apply(VariantPhotoRemoved @event, ProductDetailsReadModel current)
    {
        var variant = current.Variants.FirstOrDefault(candidate => candidate.Id == @event.VariantId);
        variant?.Photos.RemoveAll(photo => photo.Id == @event.MediaAssetId);
        return current;
    }
}
