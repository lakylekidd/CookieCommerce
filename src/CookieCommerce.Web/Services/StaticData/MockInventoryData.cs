using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Products;
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
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryChocolateChunk),
        BatchReference = "REF-225",
        BatchStatus = BatchStatus.Mixing,
        CreatedOn = DateTime.Now.AddMinutes(-4),
        PlannedQuantity = 30
    };
    // This is a batch that is currently resting in the production process. It was created 20 minutes ago and has a planned quantity of 24 cookies.
    public static BatchDto BatchRedVelvetCookie = new()
    {
        Id = Guid.NewGuid(),
        Product = ConstructBatchProductDto(StaticInventoryList.InventoryRedVelvet),
        BatchReference = "REF-226",
        BatchStatus = BatchStatus.Resting,
        CreatedOn = DateTime.Now.AddMinutes(-20),
        PlannedQuantity = 20,
        BatchRuns = 
        [
            new BatchRunDto
            {
                Id = Guid.NewGuid(),
                BatchRunReference = "RUN-001",
                Status = BatchRunStatus.Baking,
                StartedOn = DateTime.Now.AddMinutes(-15)
            },
            new BatchRunDto
            {
                Id = Guid.NewGuid(),
                BatchRunReference = "RUN-002",
                Status = BatchRunStatus.Cooling,
                StartedOn = DateTime.Now.AddMinutes(-45),
                CompletedOn = DateTime.Now.AddMinutes(-30)
            }
        ]
    };

    public static InventoryQuotaItemDto InventoryChocolateChipCookie = new()
    {
        Product = Construct(StaticInventoryList.InventoryChocolateChunk),
        QuantityReadyForFulfillment = 0,
        QuantityRequired = 30,
        Status = InventoryQuotaItemStatus.NotStarted
    };
    public static InventoryQuotaItemDto InventoryRedVelvet = new()
    {
        Product = Construct(StaticInventoryList.InventoryRedVelvet),
        QuantityReadyForFulfillment = 12,
        QuantityRequired = 20,
        Status = InventoryQuotaItemStatus.InProgress
    };

    public static InventoryAreaDto InventoryArea = new()
    {
        OrderQuota = OrderQuotaToday,
        InventoryQuota = 
        [
            InventoryChocolateChipCookie, 
            InventoryRedVelvet
        ],
        Batches = 
        [ 
            BatchChocolateChipCookie, 
            BatchRedVelvetCookie 
        ]
    };

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
            Name = product.ProductName,
            ImageUrl = product.ProductMainImage!.Url
        };
    }
}

