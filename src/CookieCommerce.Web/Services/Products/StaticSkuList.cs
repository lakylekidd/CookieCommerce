using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Products;

public static class StaticSkuList
{
    public static SkuDto SkuChocolateChunkCookie = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-111111111111"),
        Name = "Chocolate Chunk Cookie",
        Description = "A delicious chocolate chunk cookie.",
        Cost = 0.4m,
        Stock = 50,
        SkuType = "Product"
    };
    public static SkuDto SkuMatchaWhiteChocolateCookie = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-222222222222"),
        Name = "Matcha White Choc Cookie",
        Description = "A delightful matcha white chocolate cookie.",
        Cost = 0.6m,
        Stock = 30,
        SkuType = "Product"
    };
    public static SkuDto SkuTigerStripeCookie = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-333333333333"),
        Name = "Tiger Stripe Cookie",
        Description = "A tasty tiger stripe cookie.",
        Cost = 0.4m,
        Stock = 20,
        SkuType = "Product"
    };
    public static SkuDto SkuVelvetTigerCookie = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
        Name = "Velvet Tiger Cookie",
        Description = "A scrumptious velvet tiger cookie.",
        Cost = 0.45m,
        Stock = 45,
        SkuType = "Product"
    };

    public static SkuDto SkuCellophaneTransparentBag = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-555555555555"),
        Name = "Cellophane Transparent Bag",
        Description = "A single cellophane bag to protect each individual cookie 10x10x10",
        Cost = 0.025m,
        Stock = 100,
        SkuType = "Packaging"
    };
    public static SkuDto SkuBoxOf4Cookies = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-666666666666"),
        Name = "Box of 4 Cookies",
        Description = "A box that can hold 4 cookies, measuring 20x20x10.",
        Cost = 0.42m,
        Stock = 20,
        SkuType = "Packaging"
    };
    public static SkuDto SkuBoxOf12Cookies = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-777777777777"),
        Name = "Box of 12 Cookies",
        Description = "A box that can hold 12 cookies, measuring 10.5 x 11.5 x 12.5",
        Cost = 0.62m,
        Stock = 25,
        SkuType = "Packaging"
    };
    public static SkuDto SkuBoxOf8Cookies = new()
    {
        Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-888888888888"),
        Name = "Box of 8 Cookies",
        Description = "A box that can hold 8 cookies, measuring 12x12x12",
        Cost = 0.94m,
        Stock = 50,
        SkuType = "Packaging"
    };

    public static List<SkuDto> Skus { get; } =
        [
            SkuChocolateChunkCookie,
            SkuMatchaWhiteChocolateCookie,
            SkuTigerStripeCookie,
            SkuVelvetTigerCookie,
            SkuCellophaneTransparentBag,
            SkuBoxOf4Cookies,
            SkuBoxOf12Cookies,
            SkuBoxOf8Cookies            
        ];
}
