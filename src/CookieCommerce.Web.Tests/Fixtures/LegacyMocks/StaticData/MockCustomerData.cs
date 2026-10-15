using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Customers;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.StaticData;

public static class MockCustomerData
{
    public static readonly ICollection<CustomerDto> Customers =
    [
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"),
            FirstName = "Emma",
            LastName = "de Vries",
            Email = "emma.devries@gmail.com",
            Phone = "+31 6 12 34 56 78",
            Gender = "Female",
            DateOfBirth = new DateTime(1992, 4, 14),
            MemberSince = new DateTime(2022, 2, 8),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 14,
            TotalSpent = 134.50m,
            PrimaryAddress = Address("Emma", "de Vries", "Damrak", "12", null, "1012 LG", "Amsterdam", "Noord-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0002-0000-0000-000000000002"),
            FirstName = "Liam",
            LastName = "van den Berg",
            Email = "liam.vandenberg@outlook.com",
            Phone = "+31 6 98 76 54 32",
            Gender = "Male",
            DateOfBirth = new DateTime(1989, 11, 3),
            MemberSince = new DateTime(2023, 5, 19),
            IsActive = true,
            IsNewsletterSubscribed = false,
            TotalOrders = 8,
            TotalSpent = 76.40m,
            PrimaryAddress = Address("Liam", "van den Berg", "Coolsingel", "45", null, "3012 AC", "Rotterdam", "Zuid-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0003-0000-0000-000000000003"),
            FirstName = "Sophie",
            LastName = "Jansen",
            Email = "sophie.jansen@icloud.com",
            Phone = "+31 6 55 44 33 22",
            Gender = "Female",
            DateOfBirth = new DateTime(1995, 7, 21),
            MemberSince = new DateTime(2021, 9, 3),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 21,
            TotalSpent = 198.90m,
            PrimaryAddress = Address("Sophie", "Jansen", "Oudegracht", "78", null, "3511 NR", "Utrecht", "Utrecht")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0004-0000-0000-000000000004"),
            FirstName = "Noah",
            LastName = "Bakker",
            Email = "n.bakker@hotmail.com",
            Phone = "+31 6 11 22 33 44",
            Gender = "Male",
            DateOfBirth = new DateTime(1998, 1, 30),
            MemberSince = new DateTime(2023, 8, 14),
            IsActive = false,
            IsNewsletterSubscribed = false,
            TotalOrders = 3,
            TotalSpent = 22.85m,
            PrimaryAddress = Address("Noah", "Bakker", "Plein", "23", null, "2511 CR", "Den Haag", "Zuid-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0005-0000-0000-000000000005"),
            FirstName = "Olivia",
            LastName = "Visser",
            Email = "olivia.visser@gmail.com",
            Phone = "+31 6 77 88 99 00",
            Gender = "Female",
            DateOfBirth = new DateTime(2000, 3, 9),
            MemberSince = new DateTime(2024, 1, 22),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 5,
            TotalSpent = 47.70m,
            PrimaryAddress = Address("Olivia", "Visser", "Eindhovenseweg", "5", null, "5611 AL", "Eindhoven", "Noord-Brabant")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0006-0000-0000-000000000006"),
            FirstName = "Lucas",
            LastName = "Smit",
            Email = "lucas.smit@ziggo.nl",
            Phone = "+31 6 23 45 67 89",
            Gender = "Male",
            DateOfBirth = new DateTime(1987, 6, 15),
            MemberSince = new DateTime(2022, 6, 30),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 17,
            TotalSpent = 156.30m,
            PrimaryAddress = Address("Lucas", "Smit", "Kalverstraat", "89", "A", "1012 NX", "Amsterdam", "Noord-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0007-0000-0000-000000000007"),
            FirstName = "Ava",
            LastName = "Meijer",
            Email = "ava.meijer@gmail.com",
            Phone = "+31 6 34 56 78 90",
            Gender = "Female",
            DateOfBirth = new DateTime(2001, 9, 27),
            MemberSince = new DateTime(2024, 3, 10),
            IsActive = true,
            IsNewsletterSubscribed = false,
            TotalOrders = 2,
            TotalSpent = 19.80m,
            PrimaryAddress = Address("Ava", "Meijer", "Herestraat", "34", null, "9711 LA", "Groningen", "Groningen")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0008-0000-0000-000000000008"),
            FirstName = "Finn",
            LastName = "de Boer",
            Email = "finn.deboer@kpn.nl",
            Phone = "+31 6 45 67 89 01",
            Gender = "Male",
            DateOfBirth = new DateTime(1994, 12, 5),
            MemberSince = new DateTime(2023, 1, 7),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 9,
            TotalSpent = 84.55m,
            PrimaryAddress = Address("Finn", "de Boer", "Nobelstraat", "12", null, "3512 EN", "Utrecht", "Utrecht")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0009-0000-0000-000000000009"),
            FirstName = "Charlotte",
            LastName = "Mulder",
            Email = "charlotte.mulder@yahoo.com",
            Phone = "+31 6 56 78 90 12",
            Gender = "Female",
            DateOfBirth = new DateTime(1999, 8, 18),
            MemberSince = new DateTime(2024, 4, 2),
            IsActive = false,
            IsNewsletterSubscribed = true,
            TotalOrders = 1,
            TotalSpent = 9.90m,
            PrimaryAddress = Address("Charlotte", "Mulder", "Witte de Withstraat", "67", null, "3012 BN", "Rotterdam", "Zuid-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0010-0000-0000-000000000010"),
            FirstName = "Thomas",
            LastName = "van Leeuwen",
            Email = "t.vanleeuwen@xs4all.nl",
            Phone = "+31 6 67 89 01 23",
            Gender = "Male",
            DateOfBirth = new DateTime(1985, 2, 28),
            MemberSince = new DateTime(2020, 11, 15),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 28,
            TotalSpent = 265.60m,
            PrimaryAddress = Address("Thomas", "van Leeuwen", "Herengracht", "234", null, "1016 BT", "Amsterdam", "Noord-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0011-0000-0000-000000000011"),
            FirstName = "Isabelle",
            LastName = "Peters",
            Email = "isabelle.peters@gmail.com",
            Phone = "+31 6 78 90 12 34",
            Gender = "Female",
            DateOfBirth = new DateTime(1993, 5, 11),
            MemberSince = new DateTime(2023, 3, 28),
            IsActive = true,
            IsNewsletterSubscribed = false,
            TotalOrders = 6,
            TotalSpent = 57.30m,
            PrimaryAddress = Address("Isabelle", "Peters", "Grote Markt", "7", null, "2011 RB", "Haarlem", "Noord-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0012-0000-0000-000000000012"),
            FirstName = "Daan",
            LastName = "Hendriks",
            Email = "daan.hendriks@telfort.nl",
            Phone = "+31 6 89 01 23 45",
            Gender = "Male",
            DateOfBirth = new DateTime(1991, 10, 22),
            MemberSince = new DateTime(2022, 9, 4),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 11,
            TotalSpent = 103.45m,
            PrimaryAddress = Address("Daan", "Hendriks", "Rechtestraat", "19", null, "5611 GS", "Eindhoven", "Noord-Brabant")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0013-0000-0000-000000000013"),
            FirstName = "Julie",
            LastName = "Vermeer",
            Email = "julie.vermeer@icloud.com",
            Phone = "+31 6 90 12 34 56",
            Gender = "Female",
            DateOfBirth = new DateTime(1997, 3, 7),
            MemberSince = new DateTime(2024, 2, 14),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 4,
            TotalSpent = 38.20m,
            PrimaryAddress = Address("Julie", "Vermeer", "Utrechtsestraat", "44", null, "1017 VN", "Amsterdam", "Noord-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0014-0000-0000-000000000014"),
            FirstName = "Max",
            LastName = "Dekker",
            Email = "max.dekker@outlook.com",
            Phone = "+31 6 01 23 45 67",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 7, 16),
            MemberSince = new DateTime(2022, 4, 19),
            IsActive = true,
            IsNewsletterSubscribed = false,
            TotalOrders = 15,
            TotalSpent = 142.75m,
            PrimaryAddress = Address("Max", "Dekker", "Frederikstraat", "3", null, "2514 LK", "Den Haag", "Zuid-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0015-0000-0000-000000000015"),
            FirstName = "Sanne",
            LastName = "Bos",
            Email = "sanne.bos@gmail.com",
            Phone = "+31 6 12 98 76 54",
            Gender = "Female",
            DateOfBirth = new DateTime(2002, 6, 30),
            MemberSince = new DateTime(2024, 5, 1),
            IsActive = false,
            IsNewsletterSubscribed = false,
            TotalOrders = 0,
            TotalSpent = 0m,
            PrimaryAddress = Address("Sanne", "Bos", "Breestraat", "56", null, "2311 CK", "Leiden", "Zuid-Holland")
        },
        new CustomerDto
        {
            Id = Guid.Parse("a1b2c3d4-0016-0000-0000-000000000016"),
            FirstName = "Lars",
            LastName = "de Groot",
            Email = "lars.degroot@upcmail.nl",
            Phone = "+31 6 43 21 09 87",
            Gender = "Male",
            DateOfBirth = new DateTime(1988, 9, 4),
            MemberSince = new DateTime(2023, 7, 11),
            IsActive = true,
            IsNewsletterSubscribed = true,
            TotalOrders = 7,
            TotalSpent = 66.15m,
            PrimaryAddress = Address("Lars", "de Groot", "Boompjes", "14", null, "3011 XD", "Rotterdam", "Zuid-Holland")
        }
    ];

    public static ICollection<OrderListItemDto> GenerateOrdersForCustomer(CustomerDto customer)
    {
        if (customer.TotalOrders == 0) return [];

        var rng = new Random(Math.Abs(customer.Id.GetHashCode()));
        var products = StaticStorefrontProductList.Cookies;

        var orders = new List<OrderListItemDto>();
        var baseDate = customer.LastOrderDate?.Date ?? DateTime.Today.AddDays(-rng.Next(1, 8));
        var spanDays = Math.Max(1, (baseDate - customer.MemberSince).TotalDays);
        var avgGap = spanDays / Math.Max(1, customer.TotalOrders - 1);

        decimal remainingSpend = customer.TotalSpent;

        for (int i = 0; i < customer.TotalOrders; i++)
        {
            var daysBack = (int)(avgGap * i) + rng.Next(0, Math.Max(1, (int)(avgGap / 3)));
            var orderDate = baseDate.AddDays(-daysBack)
                .AddHours(rng.Next(8, 21))
                .AddMinutes(rng.Next(0, 60));

            int productCount = rng.Next(1, Math.Min(4, products.Count + 1));
            var picked = products.OrderBy(_ => rng.Next()).Take(productCount).ToList();

            decimal orderTotal;
            if (i == customer.TotalOrders - 1)
                orderTotal = Math.Round(Math.Max(remainingSpend, 9.90m), 2);
            else
                orderTotal = Math.Round(remainingSpend * (decimal)(0.05 + rng.NextDouble() * 0.20), 2);
            remainingSpend -= orderTotal;

            var statusId = i == 0 && orderDate > DateTime.Today.AddDays(-7)
                ? rng.Next(0, 2)
                : 2;
            var statusLabel = statusId switch { 0 => "Processing", 1 => "In Transit", _ => "Delivered" };

            orders.Add(new OrderListItemDto
            {
                Id = Guid.NewGuid(),
                OrderReference = $"TGR-{rng.Next(1000, 9999)}",
                PlacedOn = orderDate,
                CurrencySymbol = "€",
                Total = orderTotal,
                Status = new OrderStatusDto { StatusId = statusId, Status = statusLabel },
                Products = picked.Select(p => new OrderListItemProductDto
                {
                    ProductName = p.Title,
                    ProductImageUrl = p.ImageUrl,
                    Quantity = rng.Next(1, 4)
                }).ToList()
            });
        }

        return [.. orders.OrderByDescending(o => o.PlacedOn)];
    }

    private static AddressBookItemDto Address(
        string firstName, string lastName,
        string street, string houseNumber, string? addition,
        string postalCode, string city, string province)
    {
        return new AddressBookItemDto
        {
            Index = 0,
            Icon = "home",
            Label = "Home",
            FirstName = firstName,
            LastName = lastName,
            Street = street,
            HouseNumber = houseNumber,
            HouseNumberAddition = addition,
            PostalCode = postalCode,
            City = city,
            Province = province,
            Country = "Netherlands",
            Phone = "",
            IsDefault = true
        };
    }
}
