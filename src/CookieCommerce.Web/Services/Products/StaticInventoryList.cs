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
        Variants = GenerateStandardVariants(50) // Passing the main stock to divide among variants
    },
    new InventoryItemWithVariantsDto
    {
        ProductId = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
        ProductReference = "COOKIE-002",
        ProductName = "Matcha White Choc",
        ProductImageUrl = "/fake-images/matcha.png",
        Stock = 30,
        Active = true,
        Variants = GenerateStandardVariants(30)
    },
    new InventoryItemWithVariantsDto
    {
        ProductId = Guid.Parse("d8f5a8e1-1234-4567-89ab-222222222222"),
        ProductReference = "COOKIE-003",
        ProductName = "Tiger Stripe",
        ProductImageUrl = "/fake-images/oatmeal-raisin.png",
        Stock = 20,
        Active = true,
        Variants = GenerateStandardVariants(20)
    },
    new InventoryItemWithVariantsDto
    {
        ProductId = Guid.Parse("d8f5a8e1-1234-4567-89ab-555555555555"),
        ProductReference = "COOKIE-004",
        ProductName = "Velvet Tiger",
        ProductImageUrl = "/fake-images/peanut-butter.png",
        Stock = 45,
        Active = true,
        Variants = GenerateStandardVariants(45)
    }
};

    // Helper method to keep the code clean
    private static List<ProductVariantDto> GenerateStandardVariants(int totalStock)
    {
        return new List<ProductVariantDto>
    {
        new() { Id = Guid.NewGuid(), Name = "Single Cookie", Price = 1.99m, Stock = totalStock, Active = true },
        new() { Id = Guid.NewGuid(), Name = "4 Cookies Box", Price = 1.89m * 4, Stock = totalStock / 4, Active = true },
        new() { Id = Guid.NewGuid(), Name = "8 Cookies Box", Price = 1.79m * 8, Stock = totalStock / 8, Active = true },
        new() { Id = Guid.NewGuid(), Name = "12 Cookies Box", Price = 1.69m * 12, Stock = totalStock / 12, Active = true }
    };
    }
}
