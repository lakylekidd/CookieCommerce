using CookieCommerce.CatalogDomain.Domain.DomainEvents;
using CookieCommerce.CatalogDomain.Domain.Exceptions;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.CatalogDomain.Domain.Aggregates;

public class CatalogAggregate : AggregateBase
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    public CatalogAggregate() { }

    public void Create(Guid id, string name, string description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Catalog item name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Catalog item description is required.", nameof(description));

        if (price <= 0)
            throw new InvalidPriceException(id, price);

        Apply(new CatalogCreated
        {
            CatalogId = id,
            Name = name,
            Description = description,
            Price = price
        });
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Catalog item name is required.", nameof(newName));

        if (!IsActive)
            throw new CannotRenameDeactivatedCatalogException(Id, newName);

        Apply(new CatalogRenamed(Id, newName));
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new InvalidPriceException(Id, newPrice);

        if (!IsActive)
            throw new CannotChangePriceOfDeactivatedCatalogException(Id, newPrice);

        Apply(new CatalogPriceChanged(Id, Price, newPrice));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return; // Already deactivated

        Apply(new CatalogDeactivated(Id));
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case CatalogCreated e:
                Handle(e);
                break;
            case CatalogRenamed e:
                Handle(e);
                break;
            case CatalogPriceChanged e:
                Handle(e);
                break;
            case CatalogDeactivated e:
                Handle(e);
                break;
        }
    }

    private void Handle(CatalogCreated e)
    {
        Id = e.CatalogId;
        Name = e.Name;
        Description = e.Description;
        Price = e.Price;
        IsActive = true;
    }

    private void Handle(CatalogRenamed e)
    {
        Name = e.NewName;
    }

    private void Handle(CatalogPriceChanged e)
    {
        Price = e.NewPrice;
    }

    private void Handle(CatalogDeactivated e)
    {
        IsActive = false;
    }
}
