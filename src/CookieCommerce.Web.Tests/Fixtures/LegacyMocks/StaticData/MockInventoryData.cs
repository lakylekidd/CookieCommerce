using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.StaticData;

public class MockInventoryData
{
    private record MockOrderLine(string CookieRef, int Quantity);
    private record MockOrder(DateOnly DeliveryDate, DateTime PlacedAt, IReadOnlyList<MockOrderLine> Lines);

    // ── Today (max 3 types, active statuses) ─────────────────────────────────

    public static BatchDto TodayChocolateChunk = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryChocolateChunk),
        BatchReference = "REF-225",
        BatchStatus = BatchStatus.Resting,
        CreatedOn = DateTime.Now.AddMinutes(-75),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today),
        PlannedQuantity = 30,
        ActualQuantity = 30,
        DefectiveQuantity = 0,
        BatchRuns =
        [
            new BatchRunDto
            {
                Id = Guid.NewGuid(),
                BatchRunReference = "RUN-003",
                Status = BatchRunStatus.Cooling,
                StartedOn = DateTime.Now.AddMinutes(-60)
            }
        ]
    };

    public static BatchDto TodayMatcha = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryMatchaWhiteChocolate),
        BatchReference = "REF-226",
        BatchStatus = BatchStatus.Resting,
        CreatedOn = DateTime.Now.AddMinutes(-55),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today),
        PlannedQuantity = 24,
        ActualQuantity = 24,
        DefectiveQuantity = 0,
        BatchRuns =
        [
            new BatchRunDto
            {
                Id = Guid.NewGuid(),
                BatchRunReference = "RUN-004",
                Status = BatchRunStatus.Cooling,
                StartedOn = DateTime.Now.AddMinutes(-40)
            }
        ]
    };

    public static BatchDto TodayRedVelvet = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryRedVelvet),
        BatchReference = "REF-227",
        BatchStatus = BatchStatus.Resting,
        CreatedOn = DateTime.Now.AddMinutes(-25),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today),
        PlannedQuantity = 20,
        ActualQuantity = 20,
        DefectiveQuantity = 0,
        BatchRuns =
        [
            new BatchRunDto
            {
                Id = Guid.NewGuid(),
                BatchRunReference = "RUN-001",
                Status = BatchRunStatus.Baking,
                StartedOn = DateTime.Now.AddMinutes(-20)
            },
            new BatchRunDto
            {
                Id = Guid.NewGuid(),
                BatchRunReference = "RUN-002",
                Status = BatchRunStatus.Cooling,
                StartedOn = DateTime.Now.AddMinutes(-50),
                CompletedOn = DateTime.Now.AddMinutes(-35)
            }
        ]
    };

    // ── Yesterday (max 4 types, all Completed) ────────────────────────────────

    public static BatchDto YesterdayHazelnutPraline = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryHazelnutPraline),
        BatchReference = "REF-228",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-1).AddHours(15),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
        PlannedQuantity = 36,
        ActualQuantity = 35,
        DefectiveQuantity = 1
    };

    public static BatchDto YesterdayCaramelSeaSalt = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryCaramelSeaSalt),
        BatchReference = "REF-229",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-1).AddHours(11),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
        PlannedQuantity = 30,
        ActualQuantity = 28,
        DefectiveQuantity = 2
    };

    public static BatchDto YesterdayCocoaWhiteChocolate = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryCocoaWhiteChocolateChip),
        BatchReference = "REF-230",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-1).AddHours(9),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
        PlannedQuantity = 24,
        ActualQuantity = 24,
        DefectiveQuantity = 0
    };

    public static BatchDto YesterdayChocolateChunk = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryChocolateChunk),
        BatchReference = "REF-231",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-1).AddHours(8),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
        PlannedQuantity = 48,
        ActualQuantity = 46,
        DefectiveQuantity = 3
    };

    // ── 2 Days Ago (max 4 types, all Completed) ───────────────────────────────

    public static BatchDto TwoDaysRedVelvet = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryRedVelvet),
        BatchReference = "REF-232",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-2).AddHours(14),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
        PlannedQuantity = 30,
        ActualQuantity = 30,
        DefectiveQuantity = 0
    };

    public static BatchDto TwoDaysMatcha = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryMatchaWhiteChocolate),
        BatchReference = "REF-233",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-2).AddHours(12),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
        PlannedQuantity = 24,
        ActualQuantity = 22,
        DefectiveQuantity = 2
    };

    public static BatchDto TwoDaysHazelnutPraline = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryHazelnutPraline),
        BatchReference = "REF-234",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-2).AddHours(10),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
        PlannedQuantity = 36,
        ActualQuantity = 34,
        DefectiveQuantity = 2
    };

    public static BatchDto TwoDaysCaramelSeaSalt = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryCaramelSeaSalt),
        BatchReference = "REF-235",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-2).AddHours(8).AddMinutes(30),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
        PlannedQuantity = 18,
        ActualQuantity = 18,
        DefectiveQuantity = 0
    };

    // ── 3 Days Ago (max 4 types, all Completed) ───────────────────────────────

    public static BatchDto ThreeDaysCocoaWhiteChocolate = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryCocoaWhiteChocolateChip),
        BatchReference = "REF-236",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-3).AddHours(15),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
        PlannedQuantity = 36,
        ActualQuantity = 35,
        DefectiveQuantity = 1
    };

    public static BatchDto ThreeDaysChocolateChunk = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryChocolateChunk),
        BatchReference = "REF-237",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(30),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
        PlannedQuantity = 48,
        ActualQuantity = 47,
        DefectiveQuantity = 1
    };

    public static BatchDto ThreeDaysRedVelvet = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryRedVelvet),
        BatchReference = "REF-238",
        BatchStatus = BatchStatus.Completed,
        CreatedOn = DateTime.Today.AddDays(-3).AddHours(9),
        DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
        PlannedQuantity = 20,
        ActualQuantity = 19,
        DefectiveQuantity = 1
    };

    // ─────────────────────────────────────────────────────────────────────────

    private static readonly IReadOnlyList<BatchDto> AllBatches =
    [
        // Today
        TodayChocolateChunk, TodayMatcha, TodayRedVelvet,
        // Yesterday
        YesterdayHazelnutPraline, YesterdayCaramelSeaSalt, YesterdayCocoaWhiteChocolate, YesterdayChocolateChunk,
        // 2 days ago
        TwoDaysRedVelvet, TwoDaysMatcha, TwoDaysHazelnutPraline, TwoDaysCaramelSeaSalt,
        // 3 days ago
        ThreeDaysCocoaWhiteChocolate, ThreeDaysChocolateChunk, ThreeDaysRedVelvet
    ];

    // ── Mock orders spread across today + 3 prior days ───────────────────────
    // Today: 8 orders, 6 ready (only Red Velvet has a cooling run with actual qty)
    // Yesterday/prior: all orders ready (all batches completed)

    private static readonly IReadOnlyList<MockOrder> MockOrders =
    [
        // Today — 8 orders
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(7).AddMinutes(0),  [new(StaticInventoryList.RedVelvetCookieReference, 3)]),
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(7).AddMinutes(15), [new(StaticInventoryList.RedVelvetCookieReference, 3)]),
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(7).AddMinutes(30), [new(StaticInventoryList.RedVelvetCookieReference, 3)]),
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(7).AddMinutes(45), [new(StaticInventoryList.RedVelvetCookieReference, 3)]),
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(8).AddMinutes(0),  [new(StaticInventoryList.RedVelvetCookieReference, 3)]),
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(8).AddMinutes(15), [new(StaticInventoryList.RedVelvetCookieReference, 3)]),
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(9).AddMinutes(0),  [new(StaticInventoryList.ChocolateChipCookieReference, 15), new(StaticInventoryList.MatchaWhiteChocCookieReference, 9)]),
        new(DateOnly.FromDateTime(DateTime.Today), DateTime.Today.AddHours(9).AddMinutes(30), [new(StaticInventoryList.ChocolateChipCookieReference, 15), new(StaticInventoryList.MatchaWhiteChocCookieReference, 15), new(StaticInventoryList.RedVelvetCookieReference, 2)]),

        // Yesterday — 6 orders, all ready
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateTime.Today.AddDays(-1).AddHours(7).AddMinutes(0),  [new(StaticInventoryList.HazelnutPralineCookieReference, 8),  new(StaticInventoryList.ChocolateChipCookieReference, 10)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateTime.Today.AddDays(-1).AddHours(7).AddMinutes(30), [new(StaticInventoryList.HazelnutPralineCookieReference, 6),  new(StaticInventoryList.CaramelSeaSaltCookieReference, 8)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateTime.Today.AddDays(-1).AddHours(8).AddMinutes(0),  [new(StaticInventoryList.CocoaWhiteChocolateChipCookieReference, 10), new(StaticInventoryList.ChocolateChipCookieReference, 12)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateTime.Today.AddDays(-1).AddHours(8).AddMinutes(30), [new(StaticInventoryList.HazelnutPralineCookieReference, 8),  new(StaticInventoryList.CocoaWhiteChocolateChipCookieReference, 8)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateTime.Today.AddDays(-1).AddHours(9).AddMinutes(0),  [new(StaticInventoryList.CaramelSeaSaltCookieReference, 10), new(StaticInventoryList.ChocolateChipCookieReference, 12)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), DateTime.Today.AddDays(-1).AddHours(9).AddMinutes(30), [new(StaticInventoryList.HazelnutPralineCookieReference, 8),  new(StaticInventoryList.CaramelSeaSaltCookieReference, 8), new(StaticInventoryList.ChocolateChipCookieReference, 9)]),

        // 2 days ago — 5 orders, all ready
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), DateTime.Today.AddDays(-2).AddHours(7).AddMinutes(0),  [new(StaticInventoryList.RedVelvetCookieReference, 8),  new(StaticInventoryList.MatchaWhiteChocCookieReference, 5)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), DateTime.Today.AddDays(-2).AddHours(7).AddMinutes(30), [new(StaticInventoryList.HazelnutPralineCookieReference, 10), new(StaticInventoryList.CaramelSeaSaltCookieReference, 6)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), DateTime.Today.AddDays(-2).AddHours(8).AddMinutes(0),  [new(StaticInventoryList.RedVelvetCookieReference, 8),  new(StaticInventoryList.HazelnutPralineCookieReference, 8)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), DateTime.Today.AddDays(-2).AddHours(8).AddMinutes(30), [new(StaticInventoryList.MatchaWhiteChocCookieReference, 8),    new(StaticInventoryList.CaramelSeaSaltCookieReference, 6)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), DateTime.Today.AddDays(-2).AddHours(9).AddMinutes(0),  [new(StaticInventoryList.RedVelvetCookieReference, 14), new(StaticInventoryList.HazelnutPralineCookieReference, 14)]),

        // 3 days ago — 5 orders, all ready
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), DateTime.Today.AddDays(-3).AddHours(7).AddMinutes(0),  [new(StaticInventoryList.ChocolateChipCookieReference, 12), new(StaticInventoryList.CocoaWhiteChocolateChipCookieReference, 8)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), DateTime.Today.AddDays(-3).AddHours(7).AddMinutes(30), [new(StaticInventoryList.ChocolateChipCookieReference, 10), new(StaticInventoryList.RedVelvetCookieReference, 6)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), DateTime.Today.AddDays(-3).AddHours(8).AddMinutes(0),  [new(StaticInventoryList.CocoaWhiteChocolateChipCookieReference, 10)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), DateTime.Today.AddDays(-3).AddHours(8).AddMinutes(30), [new(StaticInventoryList.ChocolateChipCookieReference, 12), new(StaticInventoryList.CocoaWhiteChocolateChipCookieReference, 8)]),
        new(DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), DateTime.Today.AddDays(-3).AddHours(9).AddMinutes(0),  [new(StaticInventoryList.ChocolateChipCookieReference, 12), new(StaticInventoryList.RedVelvetCookieReference, 6), new(StaticInventoryList.CocoaWhiteChocolateChipCookieReference, 8)]),
    ];

    // ─────────────────────────────────────────────────────────────────────────

    public static Dictionary<string, int> GetAvailableStockForDate(DateOnly date)
    {
        var batchesForDate = AllBatches.Where(b => b.DeliveryDate == date).ToList();
        return batchesForDate
            .Where(b => b.BatchRuns.Any(r => r.Status >= BatchRunStatus.Cooling))
            .GroupBy(b => b.Product.Reference)
            .ToDictionary(
                g => g.Key,
                g => Math.Max(0, g.Sum(b => b.ActualQuantity - b.DefectiveQuantity)));
    }

    public static InventoryAreaDto GetInventoryForDate(DateOnly date)
    {
        var ordersForDate = MockOrders.Where(o => o.DeliveryDate == date).ToList();

        var available = GetAvailableStockForDate(date);

        int bakedToday = available.Values.Sum();
        int totalCookiesNeeded = ordersForDate.SelectMany(o => o.Lines).Sum(l => l.Quantity);
        const int currentQuota = 50;

        var orderQuota = new OrderQuotaDto
        {
            QuotaDate = date.ToDateTime(TimeOnly.MinValue),
            TotalOrders = ordersForDate.Count,
            OrdersReadyToFulfill = ComputeOrdersReadyToFulfill(ordersForDate, available),
            CurrentQuota = currentQuota,
            BakedToday = bakedToday,
            RemainingToBake = Math.Max(0, totalCookiesNeeded - bakedToday),
            DailyQuotaPercentage = currentQuota > 0
                ? Math.Min(1.0, bakedToday / (double)currentQuota)
                : 0
        };

        return new InventoryAreaDto
        {
            OrderQuota = orderQuota,
            InventoryQuota = ComputeInventoryQuota(ordersForDate, available),
            Batches = AllBatches.ToList()
        };
    }

    private static int ComputeOrdersReadyToFulfill(
        List<MockOrder> orders,
        Dictionary<string, int> available)
    {
        var stock = new Dictionary<string, int>(available);
        int readyCount = 0;
        foreach (var order in orders.OrderBy(o => o.PlacedAt))
        {
            bool orderReady = order.Lines.All(l =>
                stock.GetValueOrDefault(l.CookieRef) >= l.Quantity);
            if (orderReady)
            {
                readyCount++;
                foreach (var line in order.Lines)
                    stock[line.CookieRef] -= line.Quantity;
            }
        }
        return readyCount;
    }

    private static List<InventoryQuotaItemDto> ComputeInventoryQuota(
        List<MockOrder> orders,
        Dictionary<string, int> available)
    {
        var cookieRefs = orders.SelectMany(o => o.Lines).Select(l => l.CookieRef).Distinct();
        var result = new List<InventoryQuotaItemDto>();
        foreach (var cookieRef in cookieRefs)
        {
            int required = orders.SelectMany(o => o.Lines)
                .Where(l => l.CookieRef == cookieRef)
                .Sum(l => l.Quantity);
            int ready = available.GetValueOrDefault(cookieRef, 0);
            var status = ready == 0 ? InventoryQuotaItemStatus.NotStarted
                : ready < required ? InventoryQuotaItemStatus.InProgress
                : InventoryQuotaItemStatus.ReadyForFulfillment;

            var product = StaticInventoryList.InventoryItems.First(p => p.ProductReference == cookieRef);
            result.Add(new InventoryQuotaItemDto
            {
                Product = Construct(product),
                QuantityRequired = required,
                QuantityReadyForFulfillment = Math.Min(ready, required),
                Status = status
            });
        }
        return result;
    }

    private static InventoryQuotaProductDto Construct(ProductDto product)
    {
        return new InventoryQuotaProductDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            ProductImageUrl = product.ProductMainImage!.Url
        };
    }

    private static BatchProductDto ConstructBatchProductDto(ProductDto product)
    {
        return new BatchProductDto
        {
            Id = product.ProductId,
            Reference = product.ProductReference,
            Name = product.ProductName,
            ImageUrl = product.ProductMainImage!.Url
        };
    }
}
