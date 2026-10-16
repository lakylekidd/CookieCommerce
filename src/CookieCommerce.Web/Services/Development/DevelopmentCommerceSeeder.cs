using System.Security.Cryptography;
using System.Text;
using CookieCommerce.DevBootstrap;
using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.Development;

/// <summary>Seeds real development documents without calling payment or email providers.</summary>
public sealed class DevelopmentCommerceSeeder(ICommerceStore store)
{
    public async Task SeedAsync(IReadOnlyList<CatalogProduct> products, IReadOnlyList<DevelopmentSeedCustomer> customers, DateOnly today)
    {
        if (products.Count == 0 || customers.Count < 10) throw new InvalidOperationException("Wait for the catalog and ten customer profiles before seeding commerce.");
        await store.ExecuteAsync(["development-seed", "inventory", "loyalty"], async tx =>
        {
            var defaults = DevelopmentSkuData.Create();
            await SeedSkusAsync(tx, defaults);
            foreach (var product in products) await SeedRecipeAsync(tx, product, defaults);
            var settings = await tx.LoadAsync<CommerceSettings>("store") ?? new();
            var revision = await tx.LoadAsync<DevelopmentSeedRevision>("orders") ?? new();
            var changeDistribution = revision.OrderDistribution < 2;
            for (var index = 0; index < 100; index++)
            {
                var key = StableId($"order:{index}").ToString();
                var existing = await tx.LoadAsync<CommerceOrder>(key);
                if (existing is not null && !existing.IsDevelopmentSeed) continue;
                var order = existing ?? CreateOrder(index, products, customers[index % customers.Count], settings, today);
                if (existing is not null && changeDistribution) ChangeDistribution(order, index, settings, today);
                foreach (var line in order.Order.Products)
                    line.ProductImageUrl = products.FirstOrDefault(p => p.Id == line.ProductId)?.MainImage?.Url ?? "/images/logo.png";
                await tx.SaveAsync(key, order);
                if (CommerceStock.IsFulfilled(order)) await SeedHistoricalProductionAsync(tx, order, products, changeDistribution);
            }
            await tx.SaveAsync("orders", new DevelopmentSeedRevision { OrderDistribution = 2 });
            await ReplenishCookiesAsync(tx, products, today);
            return true;
        });
    }

    public static Guid StableId(string key) => new(SHA256.HashData(Encoding.UTF8.GetBytes($"CookieCommerce.Development.v1:{key}")).AsSpan(0, 16));

    private static async Task SeedSkusAsync(ICommerceTransaction tx, IReadOnlyList<SkuDto> defaults)
    {
        foreach (var entry in defaults)
        {
            var sku = await tx.LoadAsync<SkuDto>(entry.Id.ToString()) ?? entry;
            if (sku.SkuType == "Packaging") sku.Stock = 100;
            sku.ProductId ??= entry.ProductId;
            await tx.SaveAsync(sku.Id.ToString(), sku);
        }
    }

    private static async Task SeedRecipeAsync(ICommerceTransaction tx, CatalogProduct product, IReadOnlyList<SkuDto> skus)
    {
        var definition = DevelopmentBootstrapData.Products.FirstOrDefault(p => p.ProductId == product.Id);
        var existing = await tx.LoadAsync<CatalogMetadata>(product.Id.ToString());
        var metadata = existing ?? new CatalogMetadata { Reference = definition?.ProductReference ?? "", Featured = true };
        var cookie = skus.SingleOrDefault(s => s.ProductId == product.Id);
        if (cookie is null) return;
        foreach (var variant in product.Variants.Where(v => !metadata.VariantSkus.ContainsKey(v.Id)))
        {
            var units = Units(variant);
            var recipe = new List<ProductVariantSkuDto> { ToRecipe(cookie, units), ToRecipe(skus.Single(s => s.Name == "Cellophane Transparent Bag"), units) };
            if (units > 1) recipe.Add(ToRecipe(skus.Single(s => s.Name == $"Box of {units} Cookies"), 1));
            metadata.VariantSkus[variant.Id] = recipe;
        }
        await tx.SaveAsync(product.Id.ToString(), metadata);
    }

    private static ProductVariantSkuDto ToRecipe(SkuDto sku, int quantity) => new()
    { Id = sku.Id, Name = sku.Name, Description = sku.Description, Ingredients = sku.Ingredients.ToList(), Cost = sku.Cost, Quantity = quantity, SkuType = sku.SkuType };
    private static int Units(string variant) => variant.StartsWith("12 ") ? 12 : variant.StartsWith("5 ") ? 5 : variant.StartsWith("8 ") ? 8 : 1;
    private static int Units(CatalogVariant variant) => Units(DevelopmentBootstrapData.Products.SelectMany(p => p.Variants)
        .FirstOrDefault(v => v.VariantId == variant.Id)?.Name ?? variant.Name);

    private static CommerceOrder CreateOrder(int index, IReadOnlyList<CatalogProduct> products, DevelopmentSeedCustomer customer, CommerceSettings settings, DateOnly today)
    {
        var random = new Random(20261014 + index);
        var id = StableId($"order:{index}");
        var status = SeedStatus(index);
        var statusName = StatusName(status);
        var delivery = status == 6 ? today.AddDays(-3 - random.Next(28)) : today;
        var placedDate = delivery.AddDays(-1 - random.Next(3));
        if (placedDate > today) placedDate = today;
        var placed = placedDate.ToDateTime(new TimeOnly(9 + random.Next(9), random.Next(60)));
        var lines = products.OrderBy(_ => random.Next()).Take(1 + random.Next(Math.Min(3, products.Count))).Select(p =>
        {
            var variant = p.Variants[random.Next(p.Variants.Count)];
            return new OrderProductDto { ProductId = p.Id, ProductVariantId = variant.Id, ProductName = p.Name, ProductVariantName = variant.Name,
                Quantity = 1 + random.Next(3), UnitsPerItem = Units(variant), UnitPrice = variant.Price, CurrencySymbol = "€",
                ProductImageUrl = p.MainImage?.Url ?? "/images/logo.png" };
        }).ToList();
        var subtotal = lines.Sum(p => p.TotalPrice);
        var order = new OrderDto
        {
            Id = id, OrderReference = $"DEMO-{index + 1:0000}", PlacedOn = DateTime.SpecifyKind(placed, DateTimeKind.Utc),
            Products = lines, CurrencySymbol = "€", ShippingAddress = customer.Address, BillingAddress = customer.Address,
            ShippingCharge = subtotal >= settings.FreeShippingThreshold ? 0 : settings.StandardShippingCost, TaxRate = settings.VatRate / 100,
            Status = new() { StatusId = status, Status = statusName },
            TrackingInformation = new() { Carrier = "Standard delivery", TrackingNumber = "", TrackingStatus = status, TrackingStatusDescription = statusName }
        };
        order.Total = decimal.Round(order.TotalPrice, 2);
        return new CommerceOrder { Id = id, IsDevelopmentSeed = true, CustomerId = customer.Id, Email = customer.Email, CartId = StableId($"cart:{index}"),
            CartKey = $"development:{id}", DeliveryDate = delivery, PaymentProvider = "Development", PaymentStatus = "Paid",
            Note = "Development seed order; no payment was taken.", RewardPointsEarned = LoyaltyRules.Earned(order.Total, settings, false), Order = order };
    }

    private static int SeedStatus(int index) => index < 90 ? 6 : index < 92 ? 5 : 4;
    private static string StatusName(int status) => status switch { 6 => "Shipped", 5 => "Ready to Ship", _ => "Processing" };

    private static void ChangeDistribution(CommerceOrder order, int index, CommerceSettings settings, DateOnly today)
    {
        var status = SeedStatus(index);
        order.Order.Status = new() { StatusId = status, Status = StatusName(status) };
        order.Order.TrackingInformation.TrackingStatus = status;
        order.Order.TrackingInformation.TrackingStatusDescription = StatusName(status);
        order.PaymentStatus = "Paid";
        order.RewardPointsEarned = LoyaltyRules.Earned(order.Order.Total, settings, false);
        // Shipped demo orders belong in history; the ten active demo orders are today's work.
        order.DeliveryDate = status == 6 ? today.AddDays(-3 - new Random(20261014 + index).Next(28)) : today;
        if (DateOnly.FromDateTime(order.Order.PlacedOn) > order.DeliveryDate)
            order.Order.PlacedOn = DateTime.SpecifyKind(order.DeliveryDate.AddDays(-1).ToDateTime(new TimeOnly(10, 0)), DateTimeKind.Utc);
    }

    private static async Task SeedHistoricalProductionAsync(ICommerceTransaction tx, CommerceOrder order, IReadOnlyList<CatalogProduct> products, bool changeDistribution)
    {
        foreach (var line in order.Order.Products.GroupBy(p => p.ProductId))
        {
            var id = StableId($"production:{order.Id}:{line.Key}");
            if (await tx.LoadAsync<BatchDto>(id.ToString()) is { } existing)
            {
                if (changeDistribution)
                {
                    existing.DeliveryDate = order.DeliveryDate;
                    await tx.SaveAsync(id.ToString(), existing);
                }
                continue;
            }
            await tx.SaveAsync(id.ToString(), Batch(id, products.Single(p => p.Id == line.Key), order.DeliveryDate,
                line.Sum(p => p.Quantity * p.UnitsPerItem), $"DEMO-{order.Order.OrderReference}-{line.Key.ToString("N")[..6]}", "Production supporting a development seed order."));
        }
    }

    private static async Task ReplenishCookiesAsync(ICommerceTransaction tx, IReadOnlyList<CatalogProduct> products, DateOnly today)
    {
        foreach (var product in products)
        {
            var id = StableId($"opening-stock:{product.Id}");
            if (await tx.LoadAsync<BatchDto>(id.ToString()) is not null) continue;
            await tx.SaveAsync(id.ToString(), Batch(id, product, today, 30, $"DEMO-STOCK-{product.Id.ToString("N")[..6]}", "Development opening stock."));
        }
        var batches = await tx.ListAsync<BatchDto>();
        var orders = await tx.ListAsync<CommerceOrder>();
        foreach (var product in products)
        {
            var baked = batches.Where(b => b.Product.Id == product.Id && b.BatchStatus == BatchStatus.Completed).Sum(b => b.ActualQuantity - b.DefectiveQuantity);
            var consumed = orders.Where(CommerceStock.IsFulfilled).SelectMany(o => o.Order.Products).Where(p => p.ProductId == product.Id).Sum(p => p.Quantity * p.UnitsPerItem);
            await tx.SaveAsync($"development:{product.Id}", new InventoryStockAdjustment { ProductId = product.Id, Quantity = 30 - baked + consumed,
                EffectiveDate = today, Reason = "Development startup balance: 30 available cookies; existing production and orders retained." });
        }
    }

    private static BatchDto Batch(Guid id, CatalogProduct product, DateOnly date, int quantity, string reference, string remarks) => new()
    {
        Id = id, BatchReference = reference, DeliveryDate = date, CreatedOn = DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(6, 0)), DateTimeKind.Utc),
        PlannedQuantity = quantity, ActualQuantity = quantity, BatchStatus = BatchStatus.Completed, Remarks = remarks,
        Product = new() { Id = product.Id, Name = product.Name, Reference = DevelopmentBootstrapData.Products.FirstOrDefault(p => p.ProductId == product.Id)?.ProductReference ?? "", ImageUrl = product.MainImage?.Url ?? "" }
    };
}
