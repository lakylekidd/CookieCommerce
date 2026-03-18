using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Products;

public static class StaticInventoryList
{
    public static string ChocolateChipCookieReference = "COOKIE-001";    
    public static string MatchaWhiteChocCookieReference = "COOKIE-002";
    public static string HazelnutPralineCookieReference = "COOKIE-003";
    public static string RedVelvetCookieReference = "COOKIE-004";
    public static string CaramelSeaSaltCookieReference = "COOKIE-005";
    public static string CocoaWhiteChocolateChipCookieReference = "COOKIE-006";


    public static ProductDto InventoryChocolateChunk = new()
    {
        ProductId = Guid.Parse("602f6345-fd5c-4a1d-a2a5-fd345f36e7f8"),
        ProductReference = ChocolateChipCookieReference,
        ProductName = "Chocolate Chunk",
        ProductMainImage = new()
        {
            Id = Guid.Parse("350a9ef0-650c-4043-82a1-859f6b858377"),
            Url = "/fake-images/cookies/cookie001/top-tr.png"
        },
        Stock = StaticSkuList.SkuChocolateChunkCookie.Stock,
        Active = true,
        IsFeatured = true,
        Variants =
        [
            new ()
            {
                Id = Guid.Parse("7f57f243-eae2-4d5b-8b63-507fc2c164fe"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("b97ebd19-f141-47ea-b87f-4f85bb878326"),
                        Url = "/fake-images/cookies/cookie001/single.png"
                    },
                ],
                Name = "Single Cookie",
                Price = 1.99m,
                Stock = StaticSkuList.SkuChocolateChunkCookie.Stock,
                Skus =
                [
                    GetSku(StaticSkuList.SkuChocolateChunkCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1),
                ]
            },
            new ()
            {
                Id = Guid.Parse("0f12599c-fdf6-4192-ba6b-5e6bcb044ebe"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("3a7da46f-7a52-4785-aa46-478152751328"),
                        Url = "/fake-images/cookies/cookie001/5-pack.png"
                    },
                ],
                Name = "5 Cookies Box",
                Price = 1.89m * 5,
                Stock = StaticSkuList.SkuChocolateChunkCookie.Stock / 5,
                Skus =
                [
                    GetSku(StaticSkuList.SkuChocolateChunkCookie, 5),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 5),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("c24b7dbf-7b39-4d48-8dac-123b77418346"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("8aadfecf-fa7a-4f9c-9159-58fc05b90443"),
                        Url = "/fake-images/cookies/cookie001/12-pack.png"
                    },
                ],
                Name = "12 Cookies Box",
                Price = 1.79m * 12,
                Stock = StaticSkuList.SkuChocolateChunkCookie.Stock / 12,
                Skus =
                [
                    GetSku(StaticSkuList.SkuChocolateChunkCookie, 12),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 12),
                    GetSku(StaticSkuList.SkuBoxOf12Cookies, 1)
                ]
            }
        ]
    };
    public static ProductDto InventoryMatchaWhiteChocolate = new()
    {
        ProductId = Guid.Parse("94143a80-d56d-4728-b77a-dd9cc048d0a9"),
        ProductReference = MatchaWhiteChocCookieReference,
        ProductName = "Matcha White Chocolate",
        ProductMainImage = new() 
        { 
            Id = Guid.Parse("d6c51ca7-6405-4f54-9cae-dfbeb8f0f3ad"),
            Url = "/fake-images/cookies/cookie002/top-tr.png"
        },
        Stock = StaticSkuList.SkuMatchaWhiteChocolateCookie.Stock,
        Active = true,
        IsFeatured = true,
        Variants =
        [
            new ()
            {
                Id = Guid.Parse("acb0218d-1862-4d49-98f2-57ebe76b4ad8"),
                Active = true, 
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("764a0417-8c83-4955-b7d3-a41849c1248b"),
                        Url = "/fake-images/cookies/cookie002/single.png"
                    },
                ],
                Name = "Single Cookie",
                Price = 1.99m,
                Stock = StaticSkuList.SkuMatchaWhiteChocolateCookie.Stock,
                Skus =
                [
                    GetSku(StaticSkuList.SkuMatchaWhiteChocolateCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("339a92e6-70d4-457f-9b96-3d855b135891"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("f39e565e-ab38-451e-a03f-0e4945b65c18"),
                        Url = "/fake-images/cookies/cookie002/5-pack.png"
                    },
                ],
                Name = "5 Cookies Box",
                Price = 1.89m * 5,
                Stock = StaticSkuList.SkuMatchaWhiteChocolateCookie.Stock / 5,
                Skus =
                [
                    GetSku(StaticSkuList.SkuMatchaWhiteChocolateCookie, 5),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 5),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("ded05834-351b-4ef3-a7a0-c7c502d7a0bb"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("5b71da3f-29e9-4591-b1bf-50ade08dacc6"),
                        Url = "/fake-images/cookies/cookie002/12-pack.png"
                    },
                ],
                Name = "12 Cookies Box",
                Price = 1.79m * 12,
                Stock = StaticSkuList.SkuMatchaWhiteChocolateCookie.Stock / 12,
                Skus =
                [
                    GetSku(StaticSkuList.SkuMatchaWhiteChocolateCookie, 12),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 12),
                    GetSku(StaticSkuList.SkuBoxOf12Cookies, 1)
                ]
            }
        ]
    };
    public static ProductDto InventoryHazelnutPraline = new()
    {
        ProductId = Guid.Parse("09f77f59-ecf2-4cc4-b00f-488ea8ef9fb0"),
        ProductReference = HazelnutPralineCookieReference,
        ProductName = "Hazelnut Praline",
        ProductMainImage = new()
        {
            Id = Guid.Parse("dc5bcf17-e279-494c-9950-d560a7b1ff40"),
            Url = "/fake-images/cookies/cookie003/top-tr.png"
        },
        Stock = StaticSkuList.SkuHazelnutPralineCookie.Stock,
        Active = true,
        Variants =
        [
            new ()
            {
                Id = Guid.Parse("684c3b3b-922d-49c0-ad88-aba7b6d98329"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("644aa94c-2f56-4d28-9941-a8535282674b"),
                        Url = "/fake-images/cookies/cookie003/single.png"
                    },
                ],
                Name = "Single Cookie",
                Price = 1.99m,
                Stock = StaticSkuList.SkuHazelnutPralineCookie.Stock,
                Skus =
                [
                    GetSku(StaticSkuList.SkuHazelnutPralineCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("f3d98016-63f0-4ec9-b7a5-68b2262e8b0e"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("6d289e28-8ae2-4ec2-a6ca-547b61a7af5a"),
                        Url = "/fake-images/cookies/cookie003/5-pack.png"
                    },
                ],
                Name = "5 Cookies Box",
                Price = 1.89m * 5,
                Stock = StaticSkuList.SkuHazelnutPralineCookie.Stock / 5,
                Skus =
                [
                    GetSku(StaticSkuList.SkuHazelnutPralineCookie, 5),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 5),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("94a0527a-c6b6-470c-94da-1c9f6eeaad1d"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("4b3e7720-c8ca-4ffc-8fd3-a105a62e9b54"),
                        Url = "/fake-images/cookies/cookie003/12-pack.png"
                    },
                ],
                Name = "12 Cookies Box",
                Price = 1.79m * 12,
                Stock = StaticSkuList.SkuHazelnutPralineCookie.Stock / 12,
                Skus =
                [
                    GetSku(StaticSkuList.SkuHazelnutPralineCookie, 12),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 12),
                    GetSku(StaticSkuList.SkuBoxOf12Cookies, 1)
                ]
            }
        ]
    };
    public static ProductDto InventoryRedVelvet = new()
    {
        ProductId = Guid.Parse("1d9a6b44-fa8f-483d-8fbf-f163258b03b8"),
        ProductReference = RedVelvetCookieReference,
        ProductName = "Red Velvet",
        ProductMainImage = new()
        {
            Id = Guid.Parse("21dcc5c5-a53c-4a45-b853-ba23f81d892e"),
            Url = "/fake-images/cookies/cookie004/top-tr.png"
        },
        Stock = StaticSkuList.SkuRedVelvetCookie.Stock,
        Active = true,
        IsFeatured = true,
        Variants =
        [
            new ()
            {
                Id = Guid.Parse("b2cdf9be-49ab-4d7e-b51b-08450e4a3bda"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("e7366d60-d706-490f-b5fb-e87f54cee163"),
                        Url = "/fake-images/cookies/cookie004/single.png"
                    },
                ],
                Name = "Single Cookie",
                Price = 1.99m,
                Stock = StaticSkuList.SkuRedVelvetCookie.Stock,
                Skus =
                [
                    GetSku(StaticSkuList.SkuRedVelvetCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("4159bc90-9749-4cf6-ad9e-26f7d472a45d"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("80488b75-5918-4b31-a301-f6f038b4cb3e"),
                        Url = "/fake-images/cookies/cookie004/5-pack.png"
                    },
                ],
                Name = "5 Cookies Box",
                Price = 1.89m * 5,
                Stock = StaticSkuList.SkuRedVelvetCookie.Stock / 5,
                Skus =
                [
                    GetSku(StaticSkuList.SkuRedVelvetCookie, 5),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 5),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            },
            new()
            {
                Id = Guid.Parse("c7507dbe-be5c-49ef-abfa-f36dc39abe44"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("0898e2ed-9741-41b3-81fa-a35d6551db6d"),
                        Url = "/fake-images/cookies/cookie004/12-pack.png"
                    },
                ],
                Name = "12 Cookies Box",
                Price = 1.79m * 12,
                Stock = StaticSkuList.SkuRedVelvetCookie.Stock / 12,
                Skus =
                [
                    GetSku(StaticSkuList.SkuRedVelvetCookie, 12),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 12),
                    GetSku(StaticSkuList.SkuBoxOf12Cookies, 1)
                ]
            }
        ]
    };
    public static ProductDto InventoryCaramelSeaSalt = new()
    {
        ProductId = Guid.Parse("01c976be-a5a5-4675-aca6-05be71466298"),
        ProductReference = CaramelSeaSaltCookieReference,
        ProductName = "Caramel Sea Salt",
        ProductMainImage = new()
        {
            Id = Guid.Parse("f333accf-bf16-4a39-8730-8e29bd78eb4e"),
            Url = "/fake-images/cookies/cookie005/top-tr.png"
        },
        Stock = StaticSkuList.SkuCaramelSeaSaltCookie.Stock,
        Active = true,
        IsLimitedEdition = true,
        Variants =
        [
            new ()
            {
                Id = Guid.Parse("cbb4b36c-ba5f-47e2-bc73-e909fd3e3509"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("832d6979-8681-48a3-a683-9a8ada606dac"),
                        Url = "/fake-images/cookies/cookie005/single.png"
                    },
                ],
                Name = "Single Cookie",
                Price = 1.99m,
                Stock = StaticSkuList.SkuCaramelSeaSaltCookie.Stock,
                Skus =
                [
                    GetSku(StaticSkuList.SkuCaramelSeaSaltCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("fcd65698-cc6f-4429-adc7-c974c7f5aadc"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("44de3ae9-f911-4c76-9c44-45bd8e432317"),
                        Url = "/fake-images/cookies/cookie005/5-pack.png"
                    },
                ],
                Name = "5 Cookies Box",
                Price = 1.89m * 5,
                Stock = StaticSkuList.SkuCaramelSeaSaltCookie.Stock / 5,
                Skus =
                [
                    GetSku(StaticSkuList.SkuCaramelSeaSaltCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("963c1d2e-bc4d-4c79-aba4-a31272ee7485"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("23b95a67-b8e1-4661-a023-ca0d2f0c6024"),
                        Url = "/fake-images/cookies/cookie005/12-pack.png"
                    },
                ],
                Name = "12 Cookies Box",
                Price = 1.79m * 12,
                Stock = StaticSkuList.SkuCaramelSeaSaltCookie.Stock / 12,
                Skus =
                [
                    GetSku(StaticSkuList.SkuCaramelSeaSaltCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            }
        ]
    };
    public static ProductDto InventoryCocoaWhiteChocolateChip = new()
    {
        ProductId = Guid.Parse("f2f8b834-5d51-47bd-a974-7e5fd25245d7"),
        ProductReference = CocoaWhiteChocolateChipCookieReference,
        ProductName = "Cocoa White Chocolate Chip",
        ProductMainImage = new()
        {
            Id = Guid.Parse("8b2ac11c-881f-4f02-bd73-c8826fa1ba62"),
            Url = "/fake-images/cookies/cookie006/top-tr.png"
        },
        Stock = StaticSkuList.SkuCocoaWhiteChocolateChipCookie.Stock,
        Active = true,
        IsFeatured = true,
        Variants =
        [
            new ()
            {
                Id = Guid.Parse("c713195f-b77e-4ab1-a18d-59ee172df92b"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("b84df3a1-1398-4968-adbe-4bb917b4e501"),
                        Url = "/fake-images/cookies/cookie006/single.png"
                    },
                ],
                Name = "Single Cookie",
                Price = 1.99m,
                Stock = StaticSkuList.SkuCocoaWhiteChocolateChipCookie.Stock,
                Skus =
                [
                    GetSku(StaticSkuList.SkuCocoaWhiteChocolateChipCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("9f01b474-a62e-49eb-81af-e28b4617f3a3"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("17d9053d-a9b2-4b39-8d21-469054775cf6"),
                        Url = "/fake-images/cookies/cookie006/5-pack.png"
                    },
                ],
                Name = "5 Cookies Box",
                Price = 1.89m * 5,
                Stock = StaticSkuList.SkuCocoaWhiteChocolateChipCookie.Stock / 5,
                Skus =
                [
                    GetSku(StaticSkuList.SkuCocoaWhiteChocolateChipCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            },
            new ()
            {
                Id = Guid.Parse("4baee4a1-9003-497c-abcc-1dccd2bfaeb5"),
                Active = true,
                CurrencySymbol = "€",
                Images =
                [
                    new ()
                    {
                        Id = Guid.Parse("abe83238-9bd9-4677-b773-9e956b4cd26e"),
                        Url = "/fake-images/cookies/cookie006/12-pack.png"
                    },
                ],
                Name = "12 Cookies Box",
                Price = 1.79m * 12,
                Stock = StaticSkuList.SkuCocoaWhiteChocolateChipCookie.Stock / 12,
                Skus =
                [
                    GetSku(StaticSkuList.SkuCocoaWhiteChocolateChipCookie, 1),
                    GetSku(StaticSkuList.SkuCellophaneTransparentBag, 1),
                    GetSku(StaticSkuList.SkuBoxOf5Cookies, 1)
                ]
            }
        ]
    };

    public static List<ProductDto> InventoryItems { get; } = new List<ProductDto>
    {
        InventoryChocolateChunk,
        InventoryMatchaWhiteChocolate,
        InventoryHazelnutPraline,
        InventoryRedVelvet,
        InventoryCaramelSeaSalt,
        InventoryCocoaWhiteChocolateChip
    };

    private static ProductVariantSkuDto GetSku(SkuDto sku, int quantity)
    {
        return new ProductVariantSkuDto()
        {
            Id = sku.Id,
            Name = sku.Name,
            Description = sku.Description,
            Ingredients = sku.Ingredients,
            Quantity = quantity,
            SkuType = sku.SkuType,
            Cost = sku.Cost
        };
    }
}
