namespace CookieCommerce.ProductDomain.Domain.ValueObjects;

public sealed class ProductVariant
{
    private readonly List<ProductMediaAsset> _photos = [];

    public ProductVariant(
        Guid id,
        string name,
        decimal price,
        string currencySymbol = "€",
        bool isActive = true,
        string? sku = null)
    {
        Id = id;
        Name = name;
        Price = price;
        CurrencySymbol = currencySymbol;
        IsActive = isActive;
        Sku = sku;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string? Sku { get; private set; }
    public decimal Price { get; private set; }
    public string CurrencySymbol { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<ProductMediaAsset> Photos => _photos.AsReadOnly();

    public void Update(string name, decimal price, string currencySymbol, bool isActive, string? sku)
    {
        Name = name;
        Price = price;
        CurrencySymbol = currencySymbol;
        IsActive = isActive;
        Sku = sku;
    }

    public void AddPhoto(ProductMediaAsset photo)
    {
        _photos.RemoveAll(existing => existing.Id == photo.Id);
        _photos.Add(photo);
    }

    public void RemovePhoto(Guid mediaAssetId)
    {
        _photos.RemoveAll(photo => photo.Id == mediaAssetId);
    }
}
