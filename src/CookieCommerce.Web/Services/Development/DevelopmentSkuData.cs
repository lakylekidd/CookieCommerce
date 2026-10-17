using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Development;

public static class DevelopmentSkuData
{
    public static IReadOnlyList<SkuDto> Create()
    {
        SkuDto SkuChocolateChunkCookie = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-111111111111"),
            ProductId = Guid.Parse("602f6345-fd5c-4a1d-a2a5-fd345f36e7f8"),
            Name = "Chocolate Chunk Cookie",
            Description = "A delicious chocolate chunk cookie.",
            Cost = 0.4m,
            Stock = 30,
            SkuType = "Product",
            Ingredients = ["Organic wheat flour", "70% Belgian dark chocolate",
                "Unsalted butter", "Brown sugar", "Free-range eggs", "Vanilla extract",
                "Maldon sea salt", "Baking soda"],
        };
        SkuDto SkuMatchaWhiteChocolateCookie = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-222222222222"),
            ProductId = Guid.Parse("94143a80-d56d-4728-b77a-dd9cc048d0a9"),
            Name = "Matcha White Chocolate Cookie",
            Description = "A delightful matcha white chocolate cookie.",
            Cost = 0.6m,
            Stock = 30,
            SkuType = "Product",
            Ingredients = ["Organic wheat flour", "Ceremonial Uji matcha powder",
                "Belgian white chocolate pearls", "Unsalted butter", "Cane sugar",
                "Free-range eggs", "Almond flour", "Baking powder"],
        };
        SkuDto SkuHazelnutPralineCookie = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-333333333333"),
            ProductId = Guid.Parse("09f77f59-ecf2-4cc4-b00f-488ea8ef9fb0"),
            Name = "Hazelnut Praline Cookie",
            Description = "A nutty hazelnut praline cookie.",
            Cost = 0.6m,
            Stock = 30,
            SkuType = "Product",
            Ingredients = ["Organic wheat flour", "Hazelnut praline paste",
                "Toasted hazelnuts", "Unsalted butter", "Brown sugar",
                "Free-range eggs", "Vanilla extract", "Baking soda"],
        };
        SkuDto SkuRedVelvetCookie = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
            ProductId = Guid.Parse("1d9a6b44-fa8f-483d-8fbf-f163258b03b8"),
            Name = "Red Velvet Cookie",
            Description = "A scrumptious red velvet cookie.",
            Cost = 0.45m,
            Stock = 30,
            SkuType = "Product",
            Ingredients = ["Organic wheat flour", "Natural red food coloring from beetroot",
                "Cocoa powder", "Unsalted butter", "Cane sugar", "Free-range eggs",
                "Cream cheese chunks", "Vanilla extract", "Maldon sea salt", "Baking powder"]
        };
        SkuDto SkuCaramelSeaSaltCookie = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-555555555555"),
            ProductId = Guid.Parse("01c976be-a5a5-4675-aca6-05be71466298"),
            Name = "Caramel Sea Salt Cookie",
            Description = "A delicious caramel sea salt cookie.",
            Cost = 0.5m,
            Stock = 30,
            SkuType = "Product",
            Ingredients = ["Organic wheat flour", "Salted caramel chunks", "Unsalted butter",
                "Brown sugar", "Free-range eggs", "Vanilla extract", "Maldon sea salt", "Baking soda"]
        };
        SkuDto SkuCocoaWhiteChocolateChipCookie = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-666666666666"),
            ProductId = Guid.Parse("f2f8b834-5d51-47bd-a974-7e5fd25245d7"),
            Name = "Cocoa White Chocolate Chip Cookie",
            Description = "A delightful cocoa white chocolate chip cookie.",
            Cost = 0.55m,
            Stock = 30,
            SkuType = "Product",
            Ingredients = ["Organic wheat flour", "Cocoa powder", "Belgian white chocolate chips",
                "Unsalted butter", "Brown sugar", "Free-range eggs", "Vanilla extract",
                "Maldon sea salt", "Baking soda"]
        };
    
        SkuDto SkuCellophaneTransparentBag = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-999999999991"),
            Name = "Cellophane Transparent Bag",
            Description = "A single cellophane bag to protect each individual cookie 10x10x10",
            Cost = 0.025m,
            Stock = 100,
            SkuType = "Packaging"
        };
        SkuDto SkuBoxOf5Cookies = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-999999999992"),
            Name = "Box of 5 Cookies",
            Description = "A box that can hold 5 cookies, measuring 20x20x10.",
            Cost = 0.42m,
            Stock = 100,
            SkuType = "Packaging"
        };
        SkuDto SkuBoxOf12Cookies = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-777777777777"),
            Name = "Box of 12 Cookies",
            Description = "A box that can hold 12 cookies, measuring 10.5 x 11.5 x 12.5",
            Cost = 0.62m,
            Stock = 100,
            SkuType = "Packaging"
        };
        SkuDto SkuBoxOf8Cookies = new()
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-888888888888"),
            Name = "Box of 8 Cookies",
            Description = "A box that can hold 8 cookies, measuring 12x12x12",
            Cost = 0.94m,
            Stock = 100,
            SkuType = "Packaging"
        };
    
        return
            [
                SkuChocolateChunkCookie,
                SkuMatchaWhiteChocolateCookie,
                SkuHazelnutPralineCookie,
                SkuRedVelvetCookie,
                SkuCaramelSeaSaltCookie,
                SkuCocoaWhiteChocolateChipCookie,
                SkuCellophaneTransparentBag,
                SkuBoxOf5Cookies,
                SkuBoxOf12Cookies,
                SkuBoxOf8Cookies
            ];
    }
}
