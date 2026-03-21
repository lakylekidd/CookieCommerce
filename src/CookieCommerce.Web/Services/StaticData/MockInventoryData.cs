using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.StaticData;

public class MockInventoryData
{
    public static OrderQuotaDto OrderQuotaToday = new()
    {
        QuotaDate = DateTime.Today,
        TotalOrders = 8,
        OrdersReadyToFulfill = 6,
        CurrentQuota = 50,
        DailyQuotaPercentage = 32.0,
        BakedToday = 12,
        RemainingToBake = 34
    };

    // This is a batch that is currently being mixed in the production process. It was created 4 minutes ago and has a planned quantity of 12 cookies.
    public static BatchDto BatchChocolateChipCookie = new()
    {
        Id = Guid.NewGuid(),
        ProductId = StaticInventoryList.InventoryChocolateChunk.ProductId,
        BatchReference = "REF-225",
        BatchStatus = BatchStatus.Mixing,
        CreatedOn = DateTime.Now.AddMinutes(-4),
        PlannedQuantity = 12
    };

    // This is a batch that is currently resting in the production process. It was created 20 minutes ago and has a planned quantity of 24 cookies.
    public static BatchDto BatchRedVelvetCookie = new()
    {
        Id = Guid.NewGuid(),
        ProductId = StaticInventoryList.InventoryRedVelvet.ProductId,
        BatchReference = "REF-226",
        BatchStatus = BatchStatus.Resting,
        CreatedOn = DateTime.Now.AddMinutes(-20),
        PlannedQuantity = 24
    };

    public static InventoryAreaDto InventoryArea = new()
    {
        OrderQuota = OrderQuotaToday,
        Batches = 
        [ 
            BatchChocolateChipCookie, 
            BatchRedVelvetCookie 
        ]
    };
}
