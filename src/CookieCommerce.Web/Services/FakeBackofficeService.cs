using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Services.StaticData;
using Microsoft.AspNetCore.Hosting;

namespace CookieCommerce.Web.Services;

public class FakeBackofficeService(IWebHostEnvironment environment) : IBackofficeService
{
    public async Task<ICollection<SkuDto>> GetSkusAsync()
    {
        var skus = StaticSkuList.Skus;
        await Task.Delay(800);
        return skus;
    }

    public Task ChangeProductStatusAsync(Guid productId, bool isActive)
    {
        StaticInventoryList.InventoryItems
            .Where(i => i.ProductId == productId)
            .ToList()
            .ForEach(i => i.Active = isActive);
        return Task.Delay(500);
    }

    public Task ChangeVariantNameAsync(Guid variantId, string name)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Name = name);
        return Task.Delay(500);
    }

    public Task UpdateProductNameAsync(Guid productId, string name, CancellationToken cancellationToken = default)
    {
        var product = StaticInventoryList.InventoryItems.FirstOrDefault(item => item.ProductId == productId);
        if (product is not null)
            product.ProductName = name.Trim();

        return Task.Delay(500, cancellationToken);
    }

    public Task ChangeVariantPriceAsync(Guid variantId, decimal price)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Price = price);
        return Task.Delay(500);
    }

    public Task ChangeVariantStatusAsync(Guid variantId, bool isActive)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Active = isActive);
        return Task.Delay(500);
    }

    public Task DeleteVariantAsync(Guid variantId)
    {
        StaticInventoryList.InventoryItems
            .ToList()
            .ForEach(i => i.Variants.ToList().RemoveAll(v => v.Id == variantId));
        return Task.Delay(500);
    }

    public async Task<ProductDto> GetInventoryItemAsync(Guid productId)
    {
        var item = StaticInventoryList.InventoryItems.FirstOrDefault(i => i.ProductId == productId);
        await Task.Delay(800);
        return item;
    }

    public async Task<ICollection<ProductDto>> GetInventoryItemsAsync()
    {
        var inventoryItems = StaticInventoryList.InventoryItems;
        await Task.Delay(800);
        return inventoryItems;
    }

    public async Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate)
    {
        var orders = StaticTodaysOrders.GetTodaysOrders;
        await Task.Delay(800);
        return new QuotaDateOrdersDto
        {
            DeliveryOn = quotaDate,
            Items = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)
                .Select(g => new QuotaOrderItemDto
                {
                    ProductId = g.Key,
                    ProductImageUrl = g.First().ProductImageUrl,
                    ProductName = g.First().ProductName,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .ToList(),
            Orders = orders.ToList()
        };
    }

    public async Task<DashboardDto> GetTotalsAsync(string period)
    {
        await Task.Delay(800);
        var dashboard = MockDashboardData.GetDashboardByPeriod(period);
        return dashboard;
    }

    public async Task<ICollection<ProductVariantSkuDto>> GetVariantSkus(Guid variantId)
    {
        var skus = StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .SelectMany(v => v.Skus)
            .ToList();
        await Task.Delay(800);
        return skus;
    }

    public Task AddVariantSkuAsync(Guid productId, Guid skuId, int quantity)
    {
        var inventoryItem = StaticInventoryList.InventoryItems.FirstOrDefault(i => i.ProductId == productId);
        if (inventoryItem != null)
        {
            var variant = inventoryItem.Variants.FirstOrDefault(v => v.Id == productId);
            if (variant != null)
            {
                var sku = StaticSkuList.Skus.FirstOrDefault(s => s.Id == skuId);
                if (sku != null)
                {
                    variant.Skus.Add(new ProductVariantSkuDto
                    {
                        Id = Guid.NewGuid(),
                        Name = sku.Name,
                        Description = sku.Description,
                        Cost = sku.Cost,
                        Quantity = quantity
                    });
                }
            }
        }
        return Task.Delay(500);
    }

    public Task RemoveVariantSku(Guid variantId, Guid skuId)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v => v.Skus.ToList().RemoveAll(s => s.Id == skuId));
        return Task.Delay(500);
    }

    public Task UpdateVariantSkuQuantity(Guid variantId, Guid skuId, int quantity)
    {
        StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .Where(v => v.Id == variantId)
            .ToList()
            .ForEach(v =>
            {
                var sku = v.Skus.FirstOrDefault(s => s.Id == skuId);
                if (sku != null)
                {
                    sku.Quantity = quantity;
                }
            });
        return Task.Delay(500);
    }

    public async Task<ProductVariantDto> GetVariantAsync(Guid variantId)
    {
        var variant = StaticInventoryList.InventoryItems
            .SelectMany(i => i.Variants)
            .FirstOrDefault(v => v.Id == variantId);
        await Task.Delay(800);
        return variant;
    }

    public Task CreateProductAsync(CreateProductDto createProductDto)
    {
        ValidateVariantRows(createProductDto.Variants.Select(v => new UpsertProductVariantDto(null, v.Name, v.Price, v.CurrencySymbol, true)).ToList());

        var newProductId = Guid.NewGuid();
        StaticInventoryList.InventoryItems.Add(new ProductDto
        {
            ProductId = newProductId,
            ProductReference = createProductDto.ProductReference,
            ProductName = createProductDto.ProductName,
            Variants = createProductDto.Variants.Select(v => new ProductVariantDto
            {
                Id = Guid.NewGuid(),
                Name = v.Name.Trim(),
                Price = v.Price,
                CurrencySymbol = v.CurrencySymbol,
                Active = false,
                Skus = []
            }).ToList()
        });
        return Task.Delay(500);
    }

    public Task SetOrderAsProcessing(Guid orderId)
    {
        ChangeStatus(orderId, "Processing", 2);
        return Task.Delay(500);
    }

    public Task SetOrderAsReadyForPickup(Guid orderId)
    {
        ChangeStatus(orderId, "Ready for Pickup", 3);
        return Task.Delay(500);
    }

    public Task UpsertProductVariantsAsync(Guid productId, IReadOnlyCollection<UpsertProductVariantDto> variants, CancellationToken cancellationToken = default)
    {
        ValidateVariantRows(variants);

        var product = StaticInventoryList.InventoryItems.FirstOrDefault(item => item.ProductId == productId);
        if (product is null)
            return Task.CompletedTask;

        var requestedIds = variants
            .Where(variant => variant.Id.HasValue)
            .Select(variant => variant.Id!.Value)
            .ToHashSet();

        product.Variants = product.Variants
            .Where(existing => requestedIds.Contains(existing.Id))
            .ToList();

        foreach (var variant in variants)
        {
            var existing = variant.Id is null ? null : product.Variants.FirstOrDefault(candidate => candidate.Id == variant.Id.Value);
            if (existing is null)
            {
                product.Variants.Add(new ProductVariantDto
                {
                    Id = variant.Id ?? Guid.NewGuid(),
                    Name = variant.Name.Trim(),
                    Price = variant.Price,
                    CurrencySymbol = variant.CurrencySymbol,
                    Active = variant.IsActive
                });
            }
            else
            {
                existing.Name = variant.Name.Trim();
                existing.Price = variant.Price;
                existing.CurrencySymbol = variant.CurrencySymbol;
                existing.Active = variant.IsActive;
            }
        }

        return Task.Delay(500, cancellationToken);
    }

    public async Task<ProductImageDto> UploadProductMainImageAsync(Guid productId, ProductMediaUploadDto upload, CancellationToken cancellationToken = default)
    {
        ValidateMediaUpload(upload);

        var image = CreateUploadedImage(productId, null, "MainImage", upload);
        await SaveFileToDiskAsync(image.Url, upload.Content, cancellationToken);

        var product = StaticInventoryList.InventoryItems.FirstOrDefault(item => item.ProductId == productId);
        if (product is not null)
            product.ProductMainImage = image;

        return image;
    }

    public Task RemoveProductMainImageAsync(Guid productId, Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var product = StaticInventoryList.InventoryItems.FirstOrDefault(item => item.ProductId == productId);
        if (product?.ProductMainImage?.Id == mediaAssetId)
            product.ProductMainImage = null;

        return Task.CompletedTask;
    }

    public async Task<ProductImageDto> UploadVariantPhotoAsync(Guid productId, Guid variantId, ProductMediaUploadDto upload, CancellationToken cancellationToken = default)
    {
        ValidateMediaUpload(upload);

        var image = CreateUploadedImage(productId, variantId, "VariantPhoto", upload);
        await SaveFileToDiskAsync(image.Url, upload.Content, cancellationToken);

        var variant = StaticInventoryList.InventoryItems.FirstOrDefault(item => item.ProductId == productId)?.Variants.FirstOrDefault(candidate => candidate.Id == variantId);
        variant?.Images.Add(image);

        return image;
    }

    public Task RemoveVariantPhotoAsync(Guid productId, Guid variantId, Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var variant = StaticInventoryList.InventoryItems.FirstOrDefault(item => item.ProductId == productId)?.Variants.FirstOrDefault(candidate => candidate.Id == variantId);
        variant?.Images.Remove(variant.Images.FirstOrDefault(image => image.Id == mediaAssetId)!);

        return Task.CompletedTask;
    }

    private static void ValidateMediaUpload(ProductMediaUploadDto upload)
    {
        if (upload.LengthBytes <= 0)
            throw new InvalidOperationException("An image file is required.");

        if (upload.LengthBytes > 5 * 1024 * 1024)
            throw new InvalidOperationException("The uploaded image exceeds the maximum size of 5 MB.");

        var accepted = string.Equals(upload.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(upload.ContentType, "image/png", StringComparison.OrdinalIgnoreCase)
            || string.Equals(upload.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase);

        if (!accepted)
            throw new InvalidOperationException("The uploaded image content type is not supported.");
    }

    private async Task SaveFileToDiskAsync(string relativeUrl, Stream content, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(environment.WebRootPath, relativeUrl.TrimStart('/'));
        var directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    private static ProductImageDto CreateUploadedImage(Guid productId, Guid? variantId, string role, ProductMediaUploadDto upload) => new()
    {
        ProductId = productId,
        VariantId = variantId,
        Role = role,
        Url = $"/media/products/{productId:N}/{upload.FileName}",
        OriginalFileName = upload.FileName,
        ContentType = upload.ContentType,
        LengthBytes = upload.LengthBytes,
        AltText = upload.AltText
    };

    private static void ValidateVariantRows(IReadOnlyCollection<UpsertProductVariantDto> variants)
    {
        if (variants.Any(variant => string.IsNullOrWhiteSpace(variant.Name)))
            throw new InvalidOperationException("Variant name is required.");

        if (variants.Any(variant => variant.Price <= 0))
            throw new InvalidOperationException("Variant price must be greater than 0.");

        var hasDuplicate = variants
            .GroupBy(variant => variant.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (hasDuplicate)
            throw new InvalidOperationException("Variant names must be unique.");
    }

    private Task ChangeStatus(Guid orderId, string newStatus, int statusId)
    {
        StaticTodaysOrders.GetTodaysOrders
            .Where(o => o.Id == orderId)
            .ToList()
            .ForEach(o => o.Status = new OrderStatusDto { StatusId = statusId, Status = newStatus });
        return Task.Delay(500);
    }
}
