using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Checkout;
using CookieCommerce.Web.Models.Order;

namespace CookieCommerce.Web.Services.StaticData;

public static class StaticOrders
{
    public static List<OrderListItemDto> Orders = new List<OrderListItemDto>
    {
        new OrderListItemDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
            OrderReference = "TC-8827",
            PlacedOn = DateTime.Now,
            Status = new OrderStatusDto
            {
                StatusId = 0,
                Status = "Processing"
            },
            Total = 34.5m,
            Products = new List<OrderListItemProductDto>
            {
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/matcha.png", 
                    ProductName = "Matcha Cookies",
                    Quantity = 3
                },
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/peanut-butter.png", 
                    ProductName = "Peanut Butter Cookies",
                    Quantity = 1
                },
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/oatmeal-raisin.png", 
                    ProductName = "Oatmeal Raisin Cookies",
                    Quantity = 2
                },
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/cocoa-chocolate-chip.png", 
                    ProductName = "Cocoa Chocolate Chip Cookies",
                    Quantity = 2
                }
            }
        },
        new OrderListItemDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-555555555555"),
            OrderReference = "TC-8823",
            PlacedOn = DateTime.Now.AddDays(-10),
            Status = new OrderStatusDto
            {
                StatusId = 1, 
                Status = "In Transit" 
            },
            Total = 24.5m,
            Products = new List<OrderListItemProductDto>
            {
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/chocolate-chip.png", 
                    ProductName = "Chocolate Chip Cookies",
                    Quantity = 2
                },
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/peanut-butter.png", 
                    ProductName = "Peanut Butter Cookies",
                    Quantity = 1
                }
            }
        },
        new OrderListItemDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-666666666666"),
            OrderReference = "TC-8801",
            PlacedOn = DateTime.Now.AddMonths(-2).AddDays(10),
            Status = new OrderStatusDto
            {
                StatusId = 2,
                Status = "Delivered"
            },
            Total = 42m,
            Products = new List<OrderListItemProductDto>
            {
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/chocolate-chip.png",
                    ProductName = "Chocolate Chip Cookies",
                    Quantity = 2
                }
            }
        },
        new OrderListItemDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-777777777777"),
            OrderReference = "TC-8755",
            PlacedOn = DateTime.Now.AddMonths(-5).AddDays(-7),
            Status = new OrderStatusDto
            {
                StatusId = 2,
                Status = "Delivered"
            },
            Total = 18.95m,
            Products = new List<OrderListItemProductDto>
            {
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/chocolate-chip.png",
                    ProductName = "Chocolate Chip Cookies",
                    Quantity = 1
                }
            }
        },
        new OrderListItemDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-888888888888"),
            OrderReference = "TC-8645",
            PlacedOn = DateTime.Now.AddMonths(-5).AddDays(-7),
            Status = new OrderStatusDto
            {
                StatusId = 3,
                Status = "Cancelled"
            },
            Total = 18.95m,
            Products = new List<OrderListItemProductDto>
            {
                new OrderListItemProductDto 
                { 
                    ProductImageUrl = "/fake-images/chocolate-chip.png", 
                    ProductName = "Chocolate Chip Cookies",
                    Quantity = 2
                }
            }
        }
    };

    public static List<OrderDto> OrdersE = new List<OrderDto>
    {
        new OrderDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-444444444444"),
            OrderReference = "TC-8827",
            PlacedOn = DateTime.Now,
            Status = new OrderStatusDto { StatusId = 0, Status = "Processing" },
            Total = 34.5m,
            CurrencySymbol = "$",
            Products = new List<OrderProductDto>
            {
                new OrderProductDto { IsLimitedEdition = true, ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/matcha.png", ProductName = "Matcha Cookies", Quantity = 3, UnitPrice = 8.5m },
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/peanut-butter.png", ProductName = "Peanut Butter Cookies", Quantity = 1, UnitPrice = 4.5m },
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/oatmeal-raisin.png", ProductName = "Oatmeal Raisin Cookies", Quantity = 2, UnitPrice = 2.25m },
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/cocoa-chocolate-chip.png", ProductName = "Cocoa Chocolate Chip Cookies", Quantity = 2, UnitPrice = 2.25m }
            },
            ShippingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" },
            BillingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" }
        },
        new OrderDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-555555555555"),
            OrderReference = "TC-8823",
            PlacedOn = DateTime.Now.AddDays(-10),
            Status = new OrderStatusDto { StatusId = 1, Status = "In Transit" },
            Total = 24.5m,
            CurrencySymbol = "$",
            Products = new List<OrderProductDto>
            {
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/chocolate-chip.png", ProductName = "Chocolate Chip Cookies", Quantity = 2, UnitPrice = 10.0m },
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/peanut-butter.png", ProductName = "Peanut Butter Cookies", Quantity = 1, UnitPrice = 4.5m }
            },
            ShippingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" },
            BillingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" }
        },
        new OrderDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-666666666666"),
            OrderReference = "TC-8801",
            PlacedOn = DateTime.Now.AddMonths(-2).AddDays(10),
            Status = new OrderStatusDto { StatusId = 2, Status = "Delivered" },
            Total = 42m,
            CurrencySymbol = "$",
            Products = new List<OrderProductDto>
            {
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/chocolate-chip.png", ProductName = "Chocolate Chip Cookies", Quantity = 2, UnitPrice = 21.0m }
            },
            ShippingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" },
            BillingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" }
        },
        new OrderDto
        {
                Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-777777777777"),
            OrderReference = "TC-8755",
            PlacedOn = DateTime.Now.AddMonths(-5).AddDays(-7),
            Status = new OrderStatusDto { StatusId = 2, Status = "Delivered" },
            Total = 18.95m,
            CurrencySymbol = "$",
            Products = new List<OrderProductDto>
            {
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/chocolate-chip.png", ProductName = "Chocolate Chip Cookies", Quantity = 1, UnitPrice = 18.95m }
            },
            ShippingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" },
            BillingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" }
        },
        new OrderDto
        {
            Id = Guid.Parse("d8f5a8e1-1234-4567-89ab-888888888888"),
            OrderReference = "TC-8645",
            PlacedOn = DateTime.Now.AddMonths(-5).AddDays(-7),
            Status = new OrderStatusDto { StatusId = 3, Status = "Cancelled" },
            Total = 18.95m,
            CurrencySymbol = "$",
            Products = new List<OrderProductDto>
            {
                new OrderProductDto { ProductId = Guid.NewGuid(), ProductImageUrl = "/fake-images/chocolate-chip.png", ProductName = "Chocolate Chip Cookies", Quantity = 2, UnitPrice = 9.475m }
            },
            ShippingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" },
            BillingAddress = new AddressBookItemDto { FirstName = "Tiger", LastName = "User", Street = "123 Cookie Lane", City = "Baker City", PostalCode = "12345" }
        }
    };
}
