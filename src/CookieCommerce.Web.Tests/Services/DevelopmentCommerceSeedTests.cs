using CookieCommerce.DevBootstrap;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Services.Development;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CookieCommerce.Web.Tests.Services;

public sealed class DevelopmentCommerceSeedTests
{
    private readonly MemoryCommerceStore store = new();
    private readonly DateOnly today = new(2026, 10, 14);
    private static IReadOnlyList<CatalogProduct> Products => DevelopmentBootstrapData.Products.Select(p => new CatalogProduct(
        p.ProductId, p.Name, p.Description, p.BasePrice, p.IsActive, new ProductImageDto { Url = p.MainImage!.SourcePath },
        p.Variants.Select(v => new CatalogVariant(v.VariantId, v.Name, v.Sku, v.Price, v.CurrencySymbol, v.IsActive, [new() { Url = "/variant-photo.png" }])).ToList())).ToList();
    private static IReadOnlyList<DevelopmentSeedCustomer> Customers => DevelopmentBootstrapData.Customers.Select(c => new DevelopmentSeedCustomer(
        DevelopmentCommerceSeeder.StableId(c.Email), c.Email, c.FirstName, c.LastName,
        new AddressBookItemDto { FirstName = c.FirstName, LastName = c.LastName, Street = c.Addresses[0].Street, HouseNumber = c.Addresses[0].HouseNumber,
            City = c.Addresses[0].City, PostalCode = c.Addresses[0].PostalCode, Country = c.Addresses[0].Country })).ToList();

    [Fact]
    public async Task Fresh_seed_restores_original_catalog_recipes_and_creates_customer_owned_orders()
    {
        Customers.Should().HaveCount(10);
        Customers.Should().Contain(c => c.Email == "admin@tiger.com").And.Contain(c => c.Email == "customer@tiger.com");
        await new DevelopmentCommerceSeeder(store).SeedAsync(Products, Customers, today);
        var orders = await store.ListAsync<CommerceOrder>();
        orders.Should().HaveCount(100);
        orders.Should().OnlyContain(o => o.IsDevelopmentSeed && o.PaymentProvider == "Development" && o.PaymentSessionId == null && o.PaymentUrl == null);
        orders.Should().OnlyContain(o => o.Order.Total == decimal.Round(o.Order.TotalPrice, 2));
        orders.Select(o => o.CustomerId).Distinct().Should().HaveCount(10);
        orders.Count(o => o.Order.Status.StatusId == 6).Should().Be(90);
        orders.Count(o => o.Order.Status.StatusId == 5).Should().Be(2);
        orders.Count(o => o.Order.Status.StatusId == 4).Should().Be(8);
        orders.Should().OnlyContain(o => o.PaymentStatus == "Paid");
        orders.SelectMany(o => o.Order.Products).Should().OnlyContain(line => line.ProductImageUrl == Products.Single(p => p.Id == line.ProductId).MainImage!.Url);
        var skus = await store.ListAsync<SkuDto>();
        skus.Should().HaveCount(10);
        skus.Select(s => s.Id).Should().OnlyHaveUniqueItems();
        skus.Where(s => s.SkuType == "Packaging").Should().OnlyContain(s => s.Stock == 100);
        skus.Should().Contain(s => s.Name == "Box of 5 Cookies");
        foreach (var product in Products)
        {
            var metadata = await store.LoadAsync<CatalogMetadata>(product.Id.ToString());
            metadata!.VariantSkus.Should().HaveCount(3);
            metadata.VariantSkus[product.Variants[1].Id].Single(s => s.SkuType == "Product").Quantity.Should().Be(5);
            metadata.VariantSkus[product.Variants[2].Id].Single(s => s.SkuType == "Product").Quantity.Should().Be(12);
        }
        (await Stocks()).Values.Should().OnlyContain(s => s == 30);
        (await store.ListAsync<CommerceEmail>()).Should().BeEmpty();
    }

    [Fact]
    public async Task Restart_replenishes_stock_without_changing_orders_recipes_sku_costs_or_production()
    {
        var seeder = new DevelopmentCommerceSeeder(store);
        await seeder.SeedAsync(Products, Customers, today);
        var order = (await store.ListAsync<CommerceOrder>()).First();
        order.Note = "Keep this edit";
        order.Order.Products.First().UnitPrice = 8.25m;
        await store.SaveAsync(order.Id.ToString(), order);
        var package = (await store.ListAsync<SkuDto>()).First(s => s.SkuType == "Packaging");
        package.Stock = 7;
        package.Cost = 1.23m;
        await store.SaveAsync(package.Id.ToString(), package);
        var metadata = await store.LoadAsync<CatalogMetadata>(Products[0].Id.ToString());
        metadata!.Reference = "EDITED";
        metadata.VariantSkus[Products[0].Variants[0].Id] = [];
        await store.SaveAsync(Products[0].Id.ToString(), metadata);
        var production = new BatchDto { Id = Guid.CreateVersion7(), Product = new() { Id = Products[0].Id }, PlannedQuantity = 80,
            ActualQuantity = 80, BatchStatus = BatchStatus.Completed, DeliveryDate = today, Remarks = "User production" };
        await store.SaveAsync(production.Id.ToString(), production);
        await seeder.SeedAsync(Products, Customers, today.AddDays(1));
        (await store.ListAsync<CommerceOrder>()).Should().HaveCount(100);
        var preserved = await store.LoadAsync<CommerceOrder>(order.Id.ToString());
        preserved!.Note.Should().Be("Keep this edit");
        preserved.Order.Products.First().UnitPrice.Should().Be(8.25m);
        preserved.DeliveryDate.Should().Be(order.DeliveryDate);
        var updatedPackage = await store.LoadAsync<SkuDto>(package.Id.ToString());
        updatedPackage!.Stock.Should().Be(100);
        updatedPackage.Cost.Should().Be(1.23m);
        (await store.LoadAsync<CatalogMetadata>(Products[0].Id.ToString()))!.VariantSkus[Products[0].Variants[0].Id].Should().BeEmpty();
        (await store.LoadAsync<BatchDto>(production.Id.ToString()))!.ActualQuantity.Should().Be(80);
        (await Stocks()).Values.Should().OnlyContain(s => s == 30);
        await seeder.SeedAsync(Products, Customers, today.AddDays(1));
        (await Stocks()).Values.Should().OnlyContain(s => s == 30);
    }

    [Theory]
    [InlineData("Production", "true")]
    [InlineData("Development", "false")]
    public async Task Bootstrap_skips_production_and_explicitly_disabled_environments(string environment, string enabled)
    {
        var scopes = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        var clients = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var host = Mock.Of<IHostEnvironment>(e => e.EnvironmentName == environment);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DevelopmentBootstrap:Enabled"] = enabled }).Build();
        using var service = new DevelopmentCommerceBootstrapService(scopes.Object, clients.Object, host, config, NullLogger<DevelopmentCommerceBootstrapService>.Instance);
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);
        scopes.VerifyNoOtherCalls();
        clients.VerifyNoOtherCalls();
    }

    private async Task<Dictionary<Guid, int>> Stocks() => CommerceStock.Available(await store.ListAsync<BatchDto>(),
        await store.ListAsync<CommerceOrder>(), await store.ListAsync<InventoryStockAdjustment>());

    [Fact]
    public async Task Existing_demo_orders_receive_the_new_distribution_once_and_live_orders_are_untouched()
    {
        var seeder = new DevelopmentCommerceSeeder(store);
        await seeder.SeedAsync(Products, Customers, today);
        foreach (var order in await store.ListAsync<CommerceOrder>())
        {
            order.Order.Status = new() { StatusId = 10, Status = "Cancelled" };
            order.PaymentStatus = "Cancelled";
            order.Note = "Retain this note";
            await store.SaveAsync(order.Id.ToString(), order);
        }
        await store.ExecuteAsync("revision", async tx => { await tx.DeleteAsync<DevelopmentSeedRevision>("orders"); return true; });
        var liveId = Guid.CreateVersion7();
        await store.SaveAsync(liveId.ToString(), new CommerceOrder { Id = liveId, PaymentStatus = "Pending", Order = new() { Id = liveId, Status = new() { StatusId = 1 } } });
        await seeder.SeedAsync(Products, Customers, today);
        var seeded = (await store.ListAsync<CommerceOrder>()).Where(o => o.IsDevelopmentSeed).ToList();
        seeded.Count(o => o.Order.Status.StatusId == 6).Should().Be(90);
        seeded.Count(o => o.Order.Status.StatusId == 5).Should().Be(2);
        seeded.Count(o => o.Order.Status.StatusId == 4).Should().Be(8);
        seeded.Should().OnlyContain(o => o.Note == "Retain this note");
        (await store.LoadAsync<CommerceOrder>(liveId.ToString()))!.PaymentStatus.Should().Be("Pending");
        var edited = seeded[0];
        edited.Order.Status = new() { StatusId = 9, Status = "Delivered" };
        await store.SaveAsync(edited.Id.ToString(), edited);
        await seeder.SeedAsync(Products, Customers, today.AddDays(1));
        (await store.LoadAsync<CommerceOrder>(edited.Id.ToString()))!.Order.Status.StatusId.Should().Be(9);
        (await store.ListAsync<CommerceOrder>()).Should().HaveCount(101);
    }

    [Fact]
    public async Task Fulfillment_uses_the_opening_stock_and_a_restart_restores_the_consumed_units()
    {
        var seeder = new DevelopmentCommerceSeeder(store);
        await seeder.SeedAsync(Products, Customers, today);
        var id = Guid.CreateVersion7();
        await store.SaveAsync(id.ToString(), new CommerceOrder { Id = id, CustomerId = Customers[0].Id, CartKey = "test-customer", PaymentStatus = "Paid", DeliveryDate = today,
            Order = new() { Id = id, Status = new() { StatusId = 5 }, Products = [new() { ProductId = Products[0].Id, Quantity = 2, UnitsPerItem = 5 }] } });
        (await Stocks())[Products[0].Id].Should().Be(20);
        await seeder.SeedAsync(Products, Customers, today);
        (await Stocks())[Products[0].Id].Should().Be(30);
        (await store.ListAsync<CommerceOrder>()).Should().HaveCount(101);
        (await store.LoadAsync<CommerceOrder>(id.ToString()))!.Order.Status.StatusId.Should().Be(5);
    }
}
