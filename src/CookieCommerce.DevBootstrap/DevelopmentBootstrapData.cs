namespace CookieCommerce.DevBootstrap;

public static class DevelopmentBootstrapData
{
    public static IReadOnlyList<DevelopmentBootstrapCustomer> Customers { get; } =
    [
        new(
            Email: "admin@tiger.com",
            FirstName: "Tiger",
            LastName: "Baker",
            Password: "Admin123!",
            Role: "Admin",
            Addresses:
            [
                new("Home", "home", "Bakerstraat", "12", null, "Utrecht", "3511 AB", "Utrecht", "Netherlands", "+31 6 1000 0001", true),
                new("Bakery", "store", "Oudegracht", "84", "A", "Utrecht", "3511 AV", "Utrecht", "Netherlands", "+31 30 100 0002", false),
                new("Office", "business", "Stationsplein", "7", null, "Utrecht", "3511 ED", "Utrecht", "Netherlands", "+31 30 100 0003", false),
                new("Parents", "family", "Lange Nieuwstraat", "22", null, "Utrecht", "3512 PH", "Utrecht", "Netherlands", "+31 6 1000 0004", false)
            ]),
        new(
            Email: "customer@tiger.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "Customer123!",
            Role: "User",
            Addresses:
            [
                new("Home", "home", "Cookie Lane", "15", null, "Amsterdam", "1012 AB", "North Holland", "Netherlands", "+31 6 2000 0001", true),
                new("Work", "business", "Keizersgracht", "241", null, "Amsterdam", "1016 EA", "North Holland", "Netherlands", "+31 20 200 0002", false),
                new("Pickup", "location_on", "Damrak", "50", "B", "Amsterdam", "1012 LL", "North Holland", "Netherlands", "+31 20 200 0003", false),
                new("Family", "family_restroom", "Nieuwe Binnenweg", "118", null, "Rotterdam", "3015 BE", "South Holland", "Netherlands", "+31 10 200 0004", false)
            ])
    ];

    public static IReadOnlyList<DevelopmentBootstrapProduct> Products { get; } =
    [
        new(
            ProductId: Guid.Parse("602f6345-fd5c-4a1d-a2a5-fd345f36e7f8"),
            ProductReference: "COOKIE-001",
            Name: "Chocolate Chunk",
            Description: "Chocolate Chunk cookie",
            BasePrice: 1.99m,
            IsActive: true,
            MainImage: Image(Guid.Parse("350a9ef0-650c-4043-82a1-859f6b858377"), "/fake-images/cookies/cookie001/top-tr.png", "Chocolate Chunk cookie"),
            Variants:
            [
                Variant("7f57f243-eae2-4d5b-8b63-507fc2c164fe", "Single Cookie", 1.99m, "b97ebd19-f141-47ea-b87f-4f85bb878326", "/fake-images/cookies/cookie001/single.png"),
                Variant("0f12599c-fdf6-4192-ba6b-5e6bcb044ebe", "5 Cookies Box", 1.89m * 5, "3a7da46f-7a52-4785-aa46-478152751328", "/fake-images/cookies/cookie001/5-pack.png"),
                Variant("c24b7dbf-7b39-4d48-8dac-123b77418346", "12 Cookies Box", 1.79m * 12, "8aadfecf-fa7a-4f9c-9159-58fc05b90443", "/fake-images/cookies/cookie001/12-pack.png")
            ]),
        new(
            ProductId: Guid.Parse("94143a80-d56d-4728-b77a-dd9cc048d0a9"),
            ProductReference: "COOKIE-002",
            Name: "Matcha White Chocolate",
            Description: "Matcha White Chocolate cookie",
            BasePrice: 1.99m,
            IsActive: true,
            MainImage: Image(Guid.Parse("d6c51ca7-6405-4f54-9cae-dfbeb8f0f3ad"), "/fake-images/cookies/cookie002/top-tr.png", "Matcha White Chocolate cookie"),
            Variants:
            [
                Variant("acb0218d-1862-4d49-98f2-57ebe76b4ad8", "Single Cookie", 1.99m, "764a0417-8c83-4955-b7d3-a41849c1248b", "/fake-images/cookies/cookie002/single.png"),
                Variant("339a92e6-70d4-457f-9b96-3d855b135891", "5 Cookies Box", 1.89m * 5, "f39e565e-ab38-451e-a03f-0e4945b65c18", "/fake-images/cookies/cookie002/5-pack.png"),
                Variant("ded05834-351b-4ef3-a7a0-c7c502d7a0bb", "12 Cookies Box", 1.79m * 12, "5b71da3f-29e9-4591-b1bf-50ade08dacc6", "/fake-images/cookies/cookie002/12-pack.png")
            ]),
        new(
            ProductId: Guid.Parse("09f77f59-ecf2-4cc4-b00f-488ea8ef9fb0"),
            ProductReference: "COOKIE-003",
            Name: "Hazelnut Praline",
            Description: "Hazelnut Praline cookie",
            BasePrice: 1.99m,
            IsActive: true,
            MainImage: Image(Guid.Parse("dc5bcf17-e279-494c-9950-d560a7b1ff40"), "/fake-images/cookies/cookie003/top-tr.png", "Hazelnut Praline cookie"),
            Variants:
            [
                Variant("684c3b3b-922d-49c0-ad88-aba7b6d98329", "Single Cookie", 1.99m, "644aa94c-2f56-4d28-9941-a8535282674b", "/fake-images/cookies/cookie003/single.png"),
                Variant("f3d98016-63f0-4ec9-b7a5-68b2262e8b0e", "5 Cookies Box", 1.89m * 5, "6d289e28-8ae2-4ec2-a6ca-547b61a7af5a", "/fake-images/cookies/cookie003/5-pack.png"),
                Variant("94a0527a-c6b6-470c-94da-1c9f6eeaad1d", "12 Cookies Box", 1.79m * 12, "4b3e7720-c8ca-4ffc-8fd3-a105a62e9b54", "/fake-images/cookies/cookie003/12-pack.png")
            ]),
        new(
            ProductId: Guid.Parse("1d9a6b44-fa8f-483d-8fbf-f163258b03b8"),
            ProductReference: "COOKIE-004",
            Name: "Red Velvet",
            Description: "Red Velvet cookie",
            BasePrice: 1.99m,
            IsActive: true,
            MainImage: Image(Guid.Parse("21dcc5c5-a53c-4a45-b853-ba23f81d892e"), "/fake-images/cookies/cookie004/top-tr.png", "Red Velvet cookie"),
            Variants:
            [
                Variant("b2cdf9be-49ab-4d7e-b51b-08450e4a3bda", "Single Cookie", 1.99m, "e7366d60-d706-490f-b5fb-e87f54cee163", "/fake-images/cookies/cookie004/single.png"),
                Variant("4159bc90-9749-4cf6-ad9e-26f7d472a45d", "5 Cookies Box", 1.89m * 5, "80488b75-5918-4b31-a301-f6f038b4cb3e", "/fake-images/cookies/cookie004/5-pack.png"),
                Variant("c7507dbe-be5c-49ef-abfa-f36dc39abe44", "12 Cookies Box", 1.79m * 12, "0898e2ed-9741-41b3-81fa-a35d6551db6d", "/fake-images/cookies/cookie004/12-pack.png")
            ]),
        new(
            ProductId: Guid.Parse("01c976be-a5a5-4675-aca6-05be71466298"),
            ProductReference: "COOKIE-005",
            Name: "Caramel Sea Salt",
            Description: "Caramel Sea Salt cookie",
            BasePrice: 1.99m,
            IsActive: true,
            MainImage: Image(Guid.Parse("f333accf-bf16-4a39-8730-8e29bd78eb4e"), "/fake-images/cookies/cookie005/top-tr.png", "Caramel Sea Salt cookie"),
            Variants:
            [
                Variant("cbb4b36c-ba5f-47e2-bc73-e909fd3e3509", "Single Cookie", 1.99m, "832d6979-8681-48a3-a683-9a8ada606dac", "/fake-images/cookies/cookie005/single.png"),
                Variant("fcd65698-cc6f-4429-adc7-c974c7f5aadc", "5 Cookies Box", 1.89m * 5, "44de3ae9-f911-4c76-9c44-45bd8e432317", "/fake-images/cookies/cookie005/5-pack.png"),
                Variant("963c1d2e-bc4d-4c79-aba4-a31272ee7485", "12 Cookies Box", 1.79m * 12, "23b95a67-b8e1-4661-a023-ca0d2f0c6024", "/fake-images/cookies/cookie005/12-pack.png")
            ]),
        new(
            ProductId: Guid.Parse("f2f8b834-5d51-47bd-a974-7e5fd25245d7"),
            ProductReference: "COOKIE-006",
            Name: "Cocoa White Chocolate Chip",
            Description: "Cocoa White Chocolate Chip cookie",
            BasePrice: 1.99m,
            IsActive: true,
            MainImage: Image(Guid.Parse("8b2ac11c-881f-4f02-bd73-c8826fa1ba62"), "/fake-images/cookies/cookie006/top-tr.png", "Cocoa White Chocolate Chip cookie"),
            Variants:
            [
                Variant("c713195f-b77e-4ab1-a18d-59ee172df92b", "Single Cookie", 1.99m, "b84df3a1-1398-4968-adbe-4bb917b4e501", "/fake-images/cookies/cookie006/single.png"),
                Variant("9f01b474-a62e-49eb-81af-e28b4617f3a3", "5 Cookies Box", 1.89m * 5, "17d9053d-a9b2-4b39-8d21-469054775cf6", "/fake-images/cookies/cookie006/5-pack.png"),
                Variant("4baee4a1-9003-497c-abcc-1dccd2bfaeb5", "12 Cookies Box", 1.79m * 12, "abe83238-9bd9-4677-b773-9e956b4cd26e", "/fake-images/cookies/cookie006/12-pack.png")
            ])
    ];

    private static DevelopmentBootstrapVariant Variant(
        string variantId,
        string name,
        decimal price,
        string photoId,
        string photoPath) => new(
            Guid.Parse(variantId),
            name,
            price,
            "€",
            true,
            null,
            [Image(Guid.Parse(photoId), photoPath, name)]);

    private static DevelopmentBootstrapMediaAsset Image(Guid id, string sourcePath, string altText) => new(id, sourcePath, altText);
}

public sealed record DevelopmentBootstrapProduct(
    Guid ProductId,
    string ProductReference,
    string Name,
    string Description,
    decimal BasePrice,
    bool IsActive,
    DevelopmentBootstrapMediaAsset? MainImage,
    IReadOnlyList<DevelopmentBootstrapVariant> Variants);

public sealed record DevelopmentBootstrapVariant(
    Guid VariantId,
    string Name,
    decimal Price,
    string CurrencySymbol,
    bool IsActive,
    string? Sku,
    IReadOnlyList<DevelopmentBootstrapMediaAsset> Photos);

public sealed record DevelopmentBootstrapMediaAsset(
    Guid MediaAssetId,
    string SourcePath,
    string? AltText);

public sealed record DevelopmentBootstrapCustomer(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Role,
    IReadOnlyList<DevelopmentBootstrapAddress> Addresses);

public sealed record DevelopmentBootstrapAddress(
    string Label,
    string? Icon,
    string Street,
    string HouseNumber,
    string? HouseNumberAddition,
    string City,
    string PostalCode,
    string Province,
    string Country,
    string Phone,
    bool IsDefault);
