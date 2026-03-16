using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Products;

public static class StaticInventoryList
{
    public static List<InventoryItemWithVariantsDto> InventoryItems { get; } = new List<InventoryItemWithVariantsDto>
{
    new InventoryItemWithVariantsDto
    {
        ProductId = Guid.Parse("d8f5a8e1-1234-4567-89ab-111111111111"),
        ProductReference = "COOKIE-001",
        ProductName = "Chocolate Chunk",
        ProductImageUrl = "/fake-images/chocolate-chip.png",
        Stock = 50,
        Active = true,
        Variants = GenerateStandardVariants(50, "Chocolate Chunk") // Passing the main stock and product name to divide among variants
    },
    new InventoryItemWithVariantsDto
    {
        ProductId = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
        ProductReference = "COOKIE-002",
        ProductName = "Matcha White Choc",
        ProductImageUrl = "/fake-images/matcha.png",
        Stock = 30,
        Active = true,
        Variants = GenerateStandardVariants(30, "Matcha White Choc")
    },
    new InventoryItemWithVariantsDto
    {
        ProductId = Guid.Parse("d8f5a8e1-1234-4567-89ab-222222222222"),
        ProductReference = "COOKIE-003",
        ProductName = "Tiger Stripe",
        ProductImageUrl = "/fake-images/oatmeal-raisin.png",
        Stock = 20,
        Active = true,
        Variants = GenerateStandardVariants(20, "Tiger Stripe")
    },
    new InventoryItemWithVariantsDto
    {
        ProductId = Guid.Parse("d8f5a8e1-1234-4567-89ab-555555555555"),
        ProductReference = "COOKIE-004",
        ProductName = "Velvet Tiger",
        ProductImageUrl = "/fake-images/peanut-butter.png",
        Stock = 45,
        Active = true,
        Variants = GenerateStandardVariants(45, "Velvet Tiger")
    }
};

    // Helper method to keep the code clean
    private static List<ProductVariantDto> GenerateStandardVariants(int totalStock, string productName)
    {
        return new List<ProductVariantDto>
        {
            new() { Skus = GenerateSkus(1, productName), Id = Guid.NewGuid(), Name = "Single Cookie", Price = 1.99m, Stock = totalStock, Active = true },
            new() { Skus = GenerateSkus(4, productName), Id = Guid.NewGuid(), Name = "4 Cookies Box", Price = 1.89m * 4, Stock = totalStock / 4, Active = true },
            new() { Skus = GenerateSkus(8, productName), Id = Guid.NewGuid(), Name = "8 Cookies Box", Price = 1.79m * 8, Stock = totalStock / 8, Active = true },
            new() { Skus = GenerateSkus(12, productName), Id = Guid.NewGuid(), Name = "12 Cookies Box", Price = 1.69m * 12, Stock = totalStock / 12, Active = true }
        };
    }

    private static List<ProductVariantSkuDto> GenerateSkus(int pieces, string productName)
    {
        var productSku = StaticSkuList.Skus.FirstOrDefault(x => x.Name.Contains(productName));
        var cellophaneSku = StaticSkuList.Skus.FirstOrDefault(x => x.Name.Contains("Cellophane Transparent Bag"));
        
        ArgumentNullException.ThrowIfNull(productSku, $"No SKU found for product name: {productName}");
        ArgumentNullException.ThrowIfNull(cellophaneSku, "No SKU found for Cellophane Transparent Bag");

        var skuList = new List<ProductVariantSkuDto>()
        {
            new()
            {
                Id = productSku.Id, 
                Name = productSku.Name,
                Description = productSku.Description,
                Quantity = pieces,
                Cost = productSku.Cost,
            },
            new()
            {
                Id = cellophaneSku.Id, 
                Name = cellophaneSku.Name,
                Description = cellophaneSku.Description,
                Quantity = pieces,
                Cost = cellophaneSku.Cost,
            }
        };

        if (pieces == 1) return skuList;

        var sku = pieces switch
        {
            4 => StaticSkuList.Skus.FirstOrDefault(x => x.Name.Contains("Box of 4 Cookies")),
            8 => StaticSkuList.Skus.FirstOrDefault(x => x.Name.Contains("Box of 8 Cookies")),
            12 => StaticSkuList.Skus.FirstOrDefault(x => x.Name.Contains("Box of 12 Cookies")),
            _ => null,
        };

        if (sku != null)
        {
            skuList.Add(new ProductVariantSkuDto
            {
                Id = sku.Id,
                Name = sku.Name,
                Description = sku.Description,
                Quantity = 1,
                Cost = sku.Cost
            });
        }

        return skuList;
    }
}
