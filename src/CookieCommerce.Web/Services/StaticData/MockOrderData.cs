namespace CookieCommerce.Web.Services.StaticData;

using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MockOrderData
{
    private static readonly Random _random = new();

    public static List<QuotaOrderDto> GetRecentOrders()
    {
        var orders = new List<QuotaOrderDto>();
        // Set the reference date to "yesterday"
        DateTime yesterday = DateTime.Now.AddDays(-1);

        string[] cities = ["Amsterdam", "Rotterdam", "Utrecht", "The Hague", "Eindhoven"];
        (string Icon, string Name)[] carriers = [("pedal_bike", "Peddler"), ("local_shipping","Easy2Send"), ("flight", "Skynet"), ("help", "Unassigned")];
        (int Id, string Name)[] statuses = [(1, "Pending"), (2, "Processing"), (3, "Shipped"), (4, "Delivered")];

        for (int i = 1; i <= 5; i++)
        {
            // Generate a random time during yesterday
            var orderTime = yesterday.Date
                .AddHours(_random.Next(0, 24))
                .AddMinutes(_random.Next(0, 60))
                .AddSeconds(_random.Next(0, 60));

            var status = statuses[_random.Next(statuses.Length)];
            var carrier = carriers[_random.Next(carriers.Length)];

            orders.Add(new QuotaOrderDto
            {
                Id = Guid.NewGuid(),
                OrderReference = $"TGR-{1000 + i}",
                PlacedOn = orderTime,
                Total = (decimal)(_random.NextDouble() * (120.0 - 15.0) + 15.0),
                Address = new QuotaOrderAddressDto
                {
                    FirstName = GetRandomName(true),
                    LastName = GetRandomName(false),
                    City = cities[_random.Next(cities.Length)],
                    Country = "Netherlands"
                },
                Status = new OrderStatusDto
                {
                    StatusId = status.Id,
                    Status = status.Name
                },
                ShippingInformation = new ShippingInformationDto
                {
                    Icon = carrier.Icon,
                    Carrier = carrier.Name,
                    TrackingNumber = $"TRK{_random.Next(100000, 999999)}",
                    EstimatedDelivery = orderTime.AddDays(2)
                },
                Items = GetRandomProducts().Select(p => new QuotaOrderItemDto
                {
                    ProductId = p.Id,
                    ProductName = p.Title,
                    Quantity = _random.Next(1, 4),
                    ProductImageUrl = p.ImageUrl
                 }).ToList()
            });
        }

        // Return sorted by most recent first
        return orders.OrderByDescending(o => o.PlacedOn).ToList();
    }

    private static ICollection<ProductDto> GetRandomProducts()
    {
        var allCookies = StaticProductList.Cookies;
        int count = _random.Next(1, 4); // Each order has 1-3 products
        return allCookies.OrderBy(_ => _random.Next()).Take(count).ToList();
    }

    private static string GetRandomName(bool isFirst)
    {
        string[] firstNames = ["Bram", "Sanne", "Lars", "Lieke", "Daan", "Thijs", "Fleur", "Noa"];
        string[] lastNames = ["De Jong", "Jansen", "Bakker", "Visser", "Smit", "Meijer", "De Boer"];
        return isFirst ? firstNames[_random.Next(firstNames.Length)] : lastNames[_random.Next(lastNames.Length)];
    }
}
