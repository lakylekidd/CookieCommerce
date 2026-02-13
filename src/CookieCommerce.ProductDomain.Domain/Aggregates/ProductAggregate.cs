using CookieCommerce.ProductDomain.Domain.DomainEvents;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.ProductDomain.Domain.Aggregates;

public class ProductAggregate : AggregateBase
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    public ProductAggregate() { }

    public void Create(Guid id, string name, string description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description is required.", nameof(description));

        if (price <= 0)
            throw new InvalidPriceException(id, price);

        Apply(new ProductCreated
        {
            ProductId = id,
            Name = name,
            Description = description,
            Price = price
        });
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Product name is required.", nameof(newName));

        if (!IsActive)
            throw new CannotRenameDeactivatedProductException(Id, newName);

        Apply(new ProductRenamed(Id, newName));
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new InvalidPriceException(Id, newPrice);

        if (!IsActive)
            throw new CannotChangePriceOfDeactivatedProductException(Id, newPrice);

        Apply(new ProductPriceChanged(Id, Price, newPrice));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return; // Already deactivated

        Apply(new ProductDeactivated(Id));
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case ProductCreated e:
                Handle(e);
                break;
            case ProductRenamed e:
                Handle(e);
                break;
            case ProductPriceChanged e:
                Handle(e);
                break;
            case ProductDeactivated e:
                Handle(e);
                break;
        }
    }

    private void Handle(ProductCreated e)
    {
        Id = e.ProductId;
        Name = e.Name;
        Description = e.Description;
        Price = e.Price;
        IsActive = true;
    }

    private void Handle(ProductRenamed e)
    {
        Name = e.NewName;
    }

    private void Handle(ProductPriceChanged e)
    {
        Price = e.NewPrice;
    }

    private void Handle(ProductDeactivated e)
    {
        IsActive = false;
    }
}
