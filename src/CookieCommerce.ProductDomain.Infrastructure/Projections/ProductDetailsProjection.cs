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
}
