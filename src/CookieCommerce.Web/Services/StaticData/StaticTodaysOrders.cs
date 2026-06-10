using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.StaticData;

public static class StaticTodaysOrders
{
    public static QuotaOrderDto TGR1001 = new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        OrderReference = "TGR-1001",
        PlacedOn = DateTime.Today.AddDays(-1).AddHours(-2),
        Total = 45.99m,
        Address = new QuotaOrderAddressDto
        {
            FirstName = "Alice",
            LastName = "Smith",
            City = "Amsterdam",
            Country = "Netherlands"
        },
        Status = new OrderStatusDto
        {
            StatusId = 4,
            Status = "Processing"
        },
        ShippingInformation = new ShippingInformationDto
        {
            Icon = "pedal_bike",
            Carrier = "Peddler",
            TrackingNumber = "TRK123456",
            EstimatedDelivery = DateTime.Today.AddDays(2)
        },
        Items = new List<QuotaOrderItemDto>
            {
                StaticStorefrontProductList.CookieChocolateChunk.ToDto(2),
                StaticStorefrontProductList.CookieMatchaWhiteChocolate.ToDto(1)
            }
    };
    public static QuotaOrderDto TGR1002 = new()
    {
        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        OrderReference = "TGR-1002",
        PlacedOn = DateTime.Today.AddDays(-1).AddHours(-5),
        Total = 14.25m,
        Address = new QuotaOrderAddressDto
        {
            FirstName = "Bram",
            LastName = "Vermeer",
            City = "Rotterdam",
            Country = "Netherlands"
        },
        Status = new OrderStatusDto {
            StatusId = 2,
            Status = "Confirmed"
        },
        ShippingInformation = new ShippingInformationDto
        {
            Icon = "local_shipping",
            Carrier = "Easy2Send",
            TrackingNumber = "E2S998877",
            EstimatedDelivery = DateTime.Today.AddDays(1)
        },
        Items = new List<QuotaOrderItemDto>
        {
            StaticStorefrontProductList.CookieCocoaWhiteChocolateChip.ToDto(3)
        }
    };
    public static QuotaOrderDto TGR1003 = new()
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        OrderReference = "TGR-1003",
        PlacedOn = DateTime.Today.AddDays(-1).AddHours(-1),
        Total = 28.50m,
        Address = new QuotaOrderAddressDto
        {
            FirstName = "Sanne",
            LastName = "De Jong",
            City = "Amsterdam",
            Country = "Netherlands"
        },
        Status = new OrderStatusDto { StatusId = 4, Status = "Processing" },
        ShippingInformation = new ShippingInformationDto
        {
            Icon = "pedal_bike",
            Carrier = "Peddler",
            TrackingNumber = "PDL-AMS-882",
            EstimatedDelivery = DateTime.Today.AddDays(1)
        },
        Items = new List<QuotaOrderItemDto>
        {
            StaticStorefrontProductList.CookieChocolateChunk.ToDto(4),
            StaticStorefrontProductList.CookieCaramelSeaSalt.ToDto(2)
        }
    };
    public static QuotaOrderDto TGR1004 = new()
    {
        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        OrderReference = "TGR-1004",
        PlacedOn = DateTime.Today.AddDays(-1).AddHours(-10),
        Total = 20.25m,
        Address = new QuotaOrderAddressDto
        {
            FirstName = "Lars",
            LastName = "Bakker",
            City = "Utrecht",
            Country = "Netherlands"
        },
        Status = new OrderStatusDto { StatusId = 2, Status = "Confirmed" },
        ShippingInformation = new ShippingInformationDto
        {
            Icon = "flight_takeoff",
            Carrier = "SkyNet",
            TrackingNumber = "SKY-776655",
            EstimatedDelivery = DateTime.Today.AddDays(2)
        },
        Items = new List<QuotaOrderItemDto>
        {
            StaticStorefrontProductList.CookieRedVelve.ToDto(2),
            StaticStorefrontProductList.CookieHazelnutPraline.ToDto(2)
        }
    };
    public static QuotaOrderDto TGR1005 = new()
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
        OrderReference = "TGR-1005",
        PlacedOn = DateTime.Today.AddDays(-1).AddHours(-3),
        Total = 15.00m,
        Address = new QuotaOrderAddressDto
        {
            FirstName = "Anika",
            LastName = "Van Vliet",
            City = "Den Haag",
            Country = "Netherlands"
        },
        Status = new OrderStatusDto { StatusId = 2, Status = "Confirmed" },
        ShippingInformation = new ShippingInformationDto
        {
            Icon = "local_shipping",
            Carrier = "Easy2Send",
            TrackingNumber = "E2S-DH-102",
            EstimatedDelivery = DateTime.Today.AddDays(1)
        },
        Items = new List<QuotaOrderItemDto>
        {
            StaticStorefrontProductList.CookieMatchaWhiteChocolate.ToDto(3)
        }
    };
    public static QuotaOrderDto TGR1006 = new()
    {
        Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
        OrderReference = "TGR-1006",
        PlacedOn = DateTime.Today.AddDays(-1).AddHours(-8),
        Total = 33.75m,
        Address = new QuotaOrderAddressDto
        {
            FirstName = "Lieke",
            LastName = "Willems",
            City = "Amsterdam",
            Country = "Netherlands"
        },
        Status = new OrderStatusDto { StatusId = 2, Status = "Confirmed" },
        ShippingInformation = new ShippingInformationDto
        {
            Icon = "pedal_bike",
            Carrier = "Peddler",
            TrackingNumber = "PDL-AMS-991",
            EstimatedDelivery = DateTime.Today.AddDays(1)
        },
        Items = new List<QuotaOrderItemDto>
        {
            StaticStorefrontProductList.CookieCocoaWhiteChocolateChip.ToDto(2),
            StaticStorefrontProductList.CookieHazelnutPraline.ToDto(5)
        }
    };

    public static List<QuotaOrderDto> GetTodaysOrders =
    [
        TGR1001,
        TGR1002,
        TGR1003,
        TGR1004,
        TGR1005,
        TGR1006
    ];

    public static QuotaOrderItemDto ToDto(this StorefrontProductDto product, int quantity)
    {
        return new QuotaOrderItemDto
        {
            ProductId = product.Id,
            ProductReference = product.ProductReference,
            ProductImageUrl = product.ImageUrl,
            ProductName = product.Title,
            Quantity = quantity
        };
    }
}
