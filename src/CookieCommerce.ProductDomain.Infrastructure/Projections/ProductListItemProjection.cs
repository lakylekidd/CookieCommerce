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
}
