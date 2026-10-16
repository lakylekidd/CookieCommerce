using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class BackofficeService(ProductApiClient api, ICommerceStore store, CommerceAccess access, IHttpClientFactory? clients = null, ProductMainImages? mainImages = null) : IBackofficeService
{
    public async Task<ICollection<ProductDto>> GetInventoryItemsAsync()
    {
        await access.RequireAdminAsync();
        var result = new List<ProductDto>();
        foreach (var product in await api.ListAsync()) result.Add(await MapAsync(product));
        return result;
    }

    public async Task<ProductDto> GetInventoryItemAsync(Guid productId)
    {
        await access.RequireAdminAsync();
        return await MapAsync(await api.GetAsync(productId) ?? throw new KeyNotFoundException("Product not found."));
    }

    private async Task<ProductDto> MapAsync(CatalogProduct product)
    {
        var meta = await store.LoadAsync<CatalogMetadata>(product.Id.ToString()) ?? new();
        var stock = CommerceStock.Available(await store.ListAsync<BatchDto>(), await store.ListAsync<CommerceOrder>(),
            await store.ListAsync<InventoryStockAdjustment>()).GetValueOrDefault(product.Id);
        return new ProductDto
        {
            ProductId = product.Id, ProductName = product.Name, ProductReference = meta.Reference,
            ProductMainImage = product.MainImage, Active = product.IsActive, IsFeatured = meta.Featured, IsLimitedEdition = meta.LimitedEdition, Stock = stock,
            Variants = product.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id, Name = v.Name, Price = v.Price, CurrencySymbol = v.CurrencySymbol,
                Active = v.IsActive, Images = v.Photos, Skus = meta.VariantSkus.GetValueOrDefault(v.Id) ?? [],
                Stock = stock / Math.Max(1, meta.VariantSkus.GetValueOrDefault(v.Id)?.Where(s => s.SkuType == "Product").Sum(s => s.Quantity) ?? 1)
            }).ToList()
        };
    }

    public async Task CreateProductAsync(CreateProductDto createProductDto)
    {
        await access.RequireAdminAsync();
        var id = await api.CreateAsync(createProductDto);
        await store.ExecuteAsync($"catalog:{id}", async tx =>
        {
            await tx.SaveAsync(id.ToString(), new CatalogMetadata { Reference = createProductDto.ProductReference.Trim(), Featured = true });
            return true;
        });
    }

    public async Task ChangeProductStatusAsync(Guid productId, bool isActive)
    { await access.RequireAdminAsync(); await api.SetStatusAsync(productId, isActive); }
    public async Task UpdateProductNameAsync(Guid productId, string name, CancellationToken cancellationToken = default)
    { await access.RequireAdminAsync(); await api.RenameAsync(productId, name, cancellationToken); }
    public async Task UpsertProductVariantsAsync(Guid productId, IReadOnlyCollection<UpsertProductVariantDto> variants, CancellationToken cancellationToken = default)
    { await access.RequireAdminAsync(); await api.SaveVariantsAsync(productId, variants, cancellationToken); }

    public async Task<ProductVariantDto> GetVariantAsync(Guid variantId) =>
        (await GetInventoryItemsAsync()).SelectMany(p => p.Variants).FirstOrDefault(v => v.Id == variantId) ?? throw new KeyNotFoundException("Variant not found.");
    public async Task<ICollection<ProductVariantSkuDto>> GetVariantSkus(Guid variantId) => (await GetVariantAsync(variantId)).Skus;

    private async Task ChangeVariantAsync(Guid variantId, Func<CatalogVariant, UpsertProductVariantDto?> change)
    {
        await access.RequireAdminAsync();
        var product = (await api.ListAsync()).FirstOrDefault(p => p.Variants.Any(v => v.Id == variantId)) ?? throw new KeyNotFoundException("Variant not found.");
        var variants = product.Variants.Select(v => v.Id == variantId ? change(v) : ToUpsert(v)).Where(v => v is not null).Cast<UpsertProductVariantDto>().ToList();
        await api.SaveVariantsAsync(product.Id, variants);
    }

    private static UpsertProductVariantDto ToUpsert(CatalogVariant v) => new(v.Id, v.Name, v.Price, v.CurrencySymbol, v.IsActive, v.Sku);
    public Task ChangeVariantStatusAsync(Guid variantId, bool isActive) => ChangeVariantAsync(variantId, v => ToUpsert(v) with { IsActive = isActive });
    public Task ChangeVariantNameAsync(Guid variantId, string name) => ChangeVariantAsync(variantId, v => ToUpsert(v) with { Name = name });
    public Task ChangeVariantPriceAsync(Guid variantId, decimal price) => ChangeVariantAsync(variantId, v => ToUpsert(v) with { Price = price });
    public Task DeleteVariantAsync(Guid variantId) => ChangeVariantAsync(variantId, _ => null);

    public async Task<ICollection<SkuDto>> GetSkusAsync()
    {
        await access.RequireAdminAsync();
        var saved = (await store.ListAsync<SkuDto>()).ToDictionary(s => s.Id);
        var stock = CommerceStock.Available(await store.ListAsync<BatchDto>(), await store.ListAsync<CommerceOrder>(), await store.ListAsync<InventoryStockAdjustment>());
        foreach (var product in await api.ListAsync())
        {
            var cookie = saved.Values.FirstOrDefault(s => s.ProductId == product.Id && s.SkuType == "Product");
            if (cookie is not null) cookie.Stock = stock.GetValueOrDefault(product.Id);
            else saved.TryAdd(product.Id, new SkuDto { Id = product.Id, ProductId = product.Id, Name = product.Name,
                Description = product.Description ?? product.Name, Stock = stock.GetValueOrDefault(product.Id) });
        }
        return saved.Values.ToList();
    }

    // The original parameter name was productId, but the UI passes a variant ID.
    public async Task AddVariantSkuAsync(Guid productId, Guid skuId, int quantity)
    {
        var sku = (await GetSkusAsync()).FirstOrDefault(s => s.Id == skuId) ?? throw new KeyNotFoundException("SKU not found.");
        await ChangeSkusAsync(productId, skus =>
        {
            if (skus.Any(s => s.Id == skuId)) throw new InvalidOperationException("This SKU is already assigned.");
            skus.Add(new ProductVariantSkuDto { Id = sku.Id, Name = sku.Name, Description = sku.Description, Cost = sku.Cost, Quantity = quantity, SkuType = sku.SkuType, Ingredients = sku.Ingredients });
        }, quantity);
    }
    public Task RemoveVariantSku(Guid variantId, Guid skuId) => ChangeSkusAsync(variantId, skus => skus.RemoveAll(s => s.Id == skuId));
    public Task UpdateVariantSkuQuantity(Guid variantId, Guid skuId, int quantity) => ChangeSkusAsync(variantId, skus =>
    {
        var sku = skus.FirstOrDefault(s => s.Id == skuId) ?? throw new KeyNotFoundException("SKU not assigned.");
        sku.Quantity = quantity;
    }, quantity);

    private async Task ChangeSkusAsync(Guid variantId, Action<List<ProductVariantSkuDto>> change, int quantity = 1)
    {
        await access.RequireAdminAsync();
        if (quantity < 1) throw new InvalidOperationException("SKU quantity must be positive.");
        var product = (await api.ListAsync()).FirstOrDefault(p => p.Variants.Any(v => v.Id == variantId)) ?? throw new KeyNotFoundException("Variant not found.");
        await store.ExecuteAsync($"catalog:{product.Id}", async tx =>
        {
            var metadata = await tx.LoadAsync<CatalogMetadata>(product.Id.ToString()) ?? new();
            var skus = metadata.VariantSkus.GetValueOrDefault(variantId) ?? [];
            change(skus);
            metadata.VariantSkus[variantId] = skus;
            await tx.SaveAsync(product.Id.ToString(), metadata);
            return true;
        });
    }

    public async Task<ProductImageDto> UploadProductMainImageAsync(Guid productId, ProductMediaUploadDto upload, CancellationToken cancellationToken = default)
    { await access.RequireAdminAsync(); return await api.UploadAsync($"api/product/{productId}/media/main-image", upload, cancellationToken); }
    public async Task RemoveProductMainImageAsync(Guid productId, Guid mediaAssetId, CancellationToken cancellationToken = default)
    { await access.RequireAdminAsync(); await api.DeleteAsync($"api/product/{productId}/media/main-image/{mediaAssetId}", cancellationToken); }
    public async Task<ProductImageDto> UploadVariantPhotoAsync(Guid productId, Guid variantId, ProductMediaUploadDto upload, CancellationToken cancellationToken = default)
    { await access.RequireAdminAsync(); return await api.UploadAsync($"api/product/{productId}/variants/{variantId}/photos", upload, cancellationToken); }
    public async Task RemoveVariantPhotoAsync(Guid productId, Guid variantId, Guid mediaAssetId, CancellationToken cancellationToken = default)
    { await access.RequireAdminAsync(); await api.DeleteAsync($"api/product/{productId}/variants/{variantId}/photos/{mediaAssetId}", cancellationToken); }

    public Task SetOrderAsProcessing(Guid orderId) => ChangeOrderStatusAsync(orderId, 2, 4, "Processing");
    public Task SetOrderAsReadyForPickup(Guid orderId) => ChangeOrderStatusAsync(orderId, 4, 5, "Ready for Pickup");
    public Task MarkDeliveredAsync(Guid orderId) => ChangeOrderStatusAsync(orderId, 5, 9, "Delivered");
    private async Task ChangeOrderStatusAsync(Guid id, int required, int next, string name)
    {
        await access.RequireAdminAsync();
        var initial = await store.LoadAsync<CommerceOrder>(id.ToString()) ?? throw new KeyNotFoundException("Order not found.");
        await store.ExecuteAsync([initial.CartKey, "inventory"], async tx =>
        {
            var order = await tx.LoadAsync<CommerceOrder>(id.ToString()) ?? throw new KeyNotFoundException("Order not found.");
            if (order.PaymentStatus != "Paid") throw new InvalidOperationException("Payment must be confirmed before fulfilment.");
            if (order.Order.Status.StatusId == next) return true;
            if (order.Order.Status.StatusId != required) throw new InvalidOperationException("This order cannot move to that status.");
            if (next == 5)
            {
                var stock = CommerceStock.Available(await tx.ListAsync<BatchDto>(), await tx.ListAsync<CommerceOrder>(),
                    await tx.ListAsync<InventoryStockAdjustment>(), order.DeliveryDate);
                foreach (var queued in (await tx.ListAsync<CommerceOrder>()).Where(o => o.PaymentStatus == "Paid" && o.DeliveryDate == order.DeliveryDate).OrderBy(o => o.Order.PlacedOn).ThenBy(o => o.Id))
                {
                    var lines = queued.Order.Products.GroupBy(p => p.ProductId).Select(g => (Id: g.Key, Quantity: g.Sum(p => p.Quantity * p.UnitsPerItem))).ToList();
                    if (queued.Id == id && lines.Any(p => stock.GetValueOrDefault(p.Id) < p.Quantity))
                        throw new InvalidOperationException("Complete enough production for this order before marking it ready.");
                    foreach (var line in lines) stock[line.Id] = Math.Max(0, stock.GetValueOrDefault(line.Id) - line.Quantity);
                    if (queued.Id == id) break;
                }
            }
            order.Order.Status = new OrderStatusDto { StatusId = next, Status = name };
            order.Order.TrackingInformation.TrackingStatus = next;
            order.Order.TrackingInformation.TrackingStatusDescription = name;
            await tx.SaveAsync(id.ToString(), order);
            return true;
        });
    }

    public async Task<QuotaDateOrdersDto> GetQuotaDateOrdersAsync(DateTime quotaDate)
    {
        await access.RequireAdminAsync();
        var saved = (await store.ListAsync<CommerceOrder>()).Where(o => o.PaymentStatus == "Paid" && o.DeliveryDate == DateOnly.FromDateTime(quotaDate)).OrderBy(o => o.Order.PlacedOn).ToList();
        if (mainImages is not null) await mainImages.ApplyAsync(saved.SelectMany(o => o.Order.Products));
        var orders = saved.Select(ToQuotaOrder).ToList();
        return new QuotaDateOrdersDto
        {
            DeliveryOn = quotaDate, Orders = orders,
            Items = orders.SelectMany(o => o.Items).GroupBy(i => i.ProductId).Select(g => new QuotaOrderItemDto
            { ProductId = g.Key, ProductReference = g.First().ProductReference, ProductName = g.First().ProductName, ProductImageUrl = g.First().ProductImageUrl, Quantity = g.Sum(i => i.Quantity) }).ToList()
        };
    }

    public static QuotaOrderDto ToQuotaOrder(CommerceOrder entry) => new()
    {
        Id = entry.Id, OrderReference = entry.Order.OrderReference, PlacedOn = entry.Order.PlacedOn, Status = entry.Order.Status,
        Address = new() { FirstName = entry.Order.ShippingAddress.FirstName, LastName = entry.Order.ShippingAddress.LastName, City = entry.Order.ShippingAddress.City, Country = entry.Order.ShippingAddress.Country },
        Total = entry.Order.Total, CurrencySymbol = "€",
        ShippingInformation = new() { Carrier = entry.Order.TrackingInformation.Carrier, EstimatedDelivery = entry.DeliveryDate.ToDateTime(TimeOnly.MinValue), TrackingNumber = entry.Order.TrackingInformation.TrackingNumber },
        Items = entry.Order.Products.Select(p => new QuotaOrderItemDto { ProductId = p.ProductId, ProductReference = p.ProductId.ToString(), ProductName = p.ProductName, ProductImageUrl = p.ProductImageUrl, Quantity = p.Quantity * p.UnitsPerItem }).ToList()
    };

    public async Task<DashboardDto> GetTotalsAsync(string period = "this week")
    {
        await access.RequireAdminAsync();
        var today = LocalDate(DateTime.UtcNow);
        var start = period.ToLowerInvariant() switch
        { "today" => today, "this month" => new DateTime(today.Year, today.Month, 1), "this year" => new DateTime(today.Year, 1, 1), _ => today.AddDays(-((int)today.DayOfWeek + 6) % 7) };
        var all = await store.ListAsync<CommerceOrder>();
        var orders = all.Where(o => o.PaymentStatus == "Paid" && LocalDate(o.Order.PlacedOn) >= start).ToList();
        if (mainImages is not null) await mainImages.ApplyAsync(orders.SelectMany(o => o.Order.Products));
        var previousStart = start.AddDays(-(today.AddDays(1) - start).Days);
        var previous = all.Where(o => o.PaymentStatus == "Paid" && LocalDate(o.Order.PlacedOn) >= previousStart && LocalDate(o.Order.PlacedOn) < start).ToList();
        var reviews = await store.ListAsync<CustomerReview>();
        var customers = clients is null ? [] : await clients.CreateClient("customer-domain-api").GetFromJsonAsync<List<CustomerRegistration>>("api/customer") ?? [];
        var currentCustomers = customers.Count(c => c.CreatedAtUtc is { } created && LocalDate(created.UtcDateTime) >= start);
        var previousCustomers = customers.Count(c => c.CreatedAtUtc is { } created && LocalDate(created.UtcDateTime) >= previousStart && LocalDate(created.UtcDateTime) < start);
        return new DashboardDto
        {
            Period = period, TotalRevenue = orders.Sum(o => o.Order.Total), TotalOrders = orders.Count,
            TotalRevenuePercentageChange = Change(orders.Sum(o => o.Order.Total), previous.Sum(o => o.Order.Total)),
            TotalOrdersPercentageChange = Change(orders.Count, previous.Count),
            TotalCustomers = currentCustomers, TotalCustomersPercentageChange = Change(currentCustomers, previousCustomers),
            AverageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Review.Rating),
            Revenue = Enumerable.Range(0, (today - start).Days + 1).Select(i => (Day: start.AddDays(i).ToString("ddd dd"), Amount: orders.Where(o => LocalDate(o.Order.PlacedOn) == start.AddDays(i)).Sum(o => o.Order.Total))).ToList(),
            TopSellingCookies = orders.SelectMany(o => o.Order.Products).GroupBy(p => p.ProductId).OrderByDescending(g => g.Sum(p => p.Quantity)).Take(5)
                .Select((g, i) => new CookieRankDto { Rank = i + 1, Name = g.First().ProductName, ImageUrl = g.First().ProductImageUrl, Revenue = g.Sum(p => p.TotalPrice), SalesCount = g.Sum(p => p.Quantity) }).ToList()
        };
    }

    private static decimal Change(decimal current, decimal previous) => previous == 0 ? 0 : decimal.Round((current - previous) / previous * 100, 1);
    private static DateTime LocalDate(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam")).Date;
    private sealed record CustomerRegistration(Guid Id, DateTimeOffset? CreatedAtUtc);
}
