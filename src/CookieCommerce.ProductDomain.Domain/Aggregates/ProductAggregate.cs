using CookieCommerce.ProductDomain.Domain.DomainEvents;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using CookieCommerce.ProductDomain.Domain.ValueObjects;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.ProductDomain.Domain.Aggregates;

public class ProductAggregate : AggregateBase
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public ProductMediaAsset? MainImage { get; private set; }
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private readonly List<ProductVariant> _variants = [];

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

    public void Activate()
    {
        if (!IsActive) Apply(new ProductActivated(Id));
    }

    public void AddVariant(Guid variantId, string name, decimal price, string currencySymbol = "€", bool isActive = true, string? sku = null)
    {
        ValidateVariantData(variantId, name, price, currencySymbol);

        if (_variants.Any(variant => VariantNamesMatch(variant.Name, name)))
            throw new DuplicateProductVariantException(Id, name);

        Apply(new ProductVariantAdded
        {
            ProductId = Id,
            VariantId = variantId,
            Name = name.Trim(),
            Price = price,
            CurrencySymbol = currencySymbol.Trim(),
            IsActive = isActive,
            Sku = string.IsNullOrWhiteSpace(sku) ? null : sku.Trim()
        });
    }

    public void UpdateVariant(Guid variantId, string name, decimal price, string currencySymbol = "€", bool isActive = true, string? sku = null)
    {
        ValidateVariantData(variantId, name, price, currencySymbol);

        if (_variants.All(variant => variant.Id != variantId))
            throw new InvalidProductVariantException($"Variant '{variantId}' does not belong to product '{Id}'.");

        if (_variants.Any(variant => variant.Id != variantId && VariantNamesMatch(variant.Name, name)))
            throw new DuplicateProductVariantException(Id, name);

        Apply(new ProductVariantUpdated
        {
            ProductId = Id,
            VariantId = variantId,
            Name = name.Trim(),
            Price = price,
            CurrencySymbol = currencySymbol.Trim(),
            IsActive = isActive,
            Sku = string.IsNullOrWhiteSpace(sku) ? null : sku.Trim()
        });
    }

    public void RemoveVariant(Guid variantId)
    {
        if (_variants.All(variant => variant.Id != variantId))
            return;

        Apply(new ProductVariantRemoved
        {
            ProductId = Id,
            VariantId = variantId
        });
    }

    public void SetMainImage(ProductMediaAsset mediaAsset)
    {
        ValidateMediaBelongsToProduct(mediaAsset, ProductMediaRole.MainImage, null);
        Apply(new ProductMainImageSet { ProductId = Id, MediaAsset = mediaAsset });
    }

    public void RemoveMainImage(Guid mediaAssetId)
    {
        if (MainImage?.Id != mediaAssetId)
            return;

        Apply(new ProductMainImageRemoved { ProductId = Id, MediaAssetId = mediaAssetId });
    }

    public void AddVariantPhoto(Guid variantId, ProductMediaAsset mediaAsset)
    {
        var variant = _variants.FirstOrDefault(candidate => candidate.Id == variantId);
        if (variant is null)
            throw new InvalidProductMediaOwnershipException(Id, mediaAsset.Id, $"variant '{variantId}' does not belong to the product");

        ValidateMediaBelongsToProduct(mediaAsset, ProductMediaRole.VariantPhoto, variantId);
        Apply(new VariantPhotoAdded { ProductId = Id, VariantId = variantId, MediaAsset = mediaAsset });
    }

    public void RemoveVariantPhoto(Guid variantId, Guid mediaAssetId)
    {
        var variant = _variants.FirstOrDefault(candidate => candidate.Id == variantId);
        if (variant is null)
            throw new InvalidProductMediaOwnershipException(Id, mediaAssetId, $"variant '{variantId}' does not belong to the product");

        if (variant.Photos.All(photo => photo.Id != mediaAssetId))
            return;

        Apply(new VariantPhotoRemoved { ProductId = Id, VariantId = variantId, MediaAssetId = mediaAssetId });
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case ProductActivated:
                IsActive = true;
                break;
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
            case ProductVariantAdded e:
                Handle(e);
                break;
            case ProductVariantUpdated e:
                Handle(e);
                break;
            case ProductVariantRemoved e:
                Handle(e);
                break;
            case ProductMainImageSet e:
                Handle(e);
                break;
            case ProductMainImageRemoved e:
                Handle(e);
                break;
            case VariantPhotoAdded e:
                Handle(e);
                break;
            case VariantPhotoRemoved e:
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

    private void Handle(ProductVariantAdded e)
    {
        _variants.Add(new ProductVariant(e.VariantId, e.Name, e.Price, e.CurrencySymbol, e.IsActive, e.Sku));
    }

    private void Handle(ProductVariantUpdated e)
    {
        var variant = _variants.FirstOrDefault(candidate => candidate.Id == e.VariantId);
        variant?.Update(e.Name, e.Price, e.CurrencySymbol, e.IsActive, e.Sku);
    }

    private void Handle(ProductVariantRemoved e)
    {
        _variants.RemoveAll(variant => variant.Id == e.VariantId);
    }

    private void Handle(ProductMainImageSet e)
    {
        MainImage = e.MediaAsset;
    }

    private void Handle(ProductMainImageRemoved e)
    {
        if (MainImage?.Id == e.MediaAssetId)
            MainImage = null;
    }

    private void Handle(VariantPhotoAdded e)
    {
        var variant = _variants.FirstOrDefault(candidate => candidate.Id == e.VariantId);
        variant?.AddPhoto(e.MediaAsset);
    }

    private void Handle(VariantPhotoRemoved e)
    {
        var variant = _variants.FirstOrDefault(candidate => candidate.Id == e.VariantId);
        variant?.RemovePhoto(e.MediaAssetId);
    }

    private static void ValidateVariantData(Guid variantId, string name, decimal price, string currencySymbol)
    {
        if (variantId == Guid.Empty)
            throw new InvalidProductVariantException("Variant id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidProductVariantException("Variant name is required.");

        if (price <= 0)
            throw new InvalidProductVariantException("Variant price must be greater than 0.");

        if (string.IsNullOrWhiteSpace(currencySymbol))
            throw new InvalidProductVariantException("Variant currency symbol is required.");
    }

    private void ValidateMediaBelongsToProduct(ProductMediaAsset mediaAsset, ProductMediaRole expectedRole, Guid? expectedVariantId)
    {
        if (mediaAsset.ProductId != Id)
            throw new InvalidProductMediaOwnershipException(Id, mediaAsset.Id, "asset belongs to a different product");

        if (mediaAsset.Role != expectedRole)
            throw new InvalidProductMediaOwnershipException(Id, mediaAsset.Id, $"asset role must be {expectedRole}");

        if (mediaAsset.VariantId != expectedVariantId)
            throw new InvalidProductMediaOwnershipException(Id, mediaAsset.Id, "asset variant scope does not match the requested target");
    }

    private static bool VariantNamesMatch(string left, string right) =>
        string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
}
