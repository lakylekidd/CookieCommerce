using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class InventoryService(ICommerceStore store, CommerceAccess access, IBackofficeService backoffice, ProductMainImages? mainImages = null) : IInventoryService
{
    public Task<InventoryAreaDto> GetInventoryAsync() => GetInventoryForDateAsync(DateOnly.FromDateTime(DateTime.Today));
    public async Task<IReadOnlyList<BatchProductDto>> GetAvailableProductsAsync() =>
        (await backoffice.GetInventoryItemsAsync()).Select(p => new BatchProductDto { Id = p.ProductId, Reference = p.ProductReference, Name = p.ProductName, ImageUrl = p.ProductMainImage?.Url ?? "" }).ToList();

    public async Task<InventoryAreaDto> GetInventoryForDateAsync(DateOnly date)
    {
        await access.RequireAdminAsync();
        var batches = (await store.ListAsync<BatchDto>()).Where(b => b.DeliveryDate == date && b.BatchStatus != BatchStatus.Cancelled).ToList();
        if (mainImages is not null) await mainImages.ApplyAsync(batches.Select(b => b.Product));
        var quota = await backoffice.GetQuotaDateOrdersAsync(date.ToDateTime(TimeOnly.MinValue));
        var stock = CommerceStock.Available(await store.ListAsync<BatchDto>(), await store.ListAsync<CommerceOrder>(),
            await store.ListAsync<InventoryStockAdjustment>(), date);
        var required = quota.Items.Sum(i => i.Quantity);
        var settings = await store.LoadAsync<CommerceSettings>("store") ?? new();
        var target = Math.Max(required, settings.Preference("dailyQuotaTarget", 50));
        var baked = batches.Where(b => b.BatchStatus == BatchStatus.Completed).Sum(b => b.ActualQuantity - b.DefectiveQuantity);
        var remaining = new Dictionary<Guid, int>(stock);
        var ready = 0;
        foreach (var order in quota.Orders.OrderBy(o => o.PlacedOn))
        {
            var lines = order.Items.GroupBy(i => i.ProductId).Select(g => (Id: g.Key, Quantity: g.Sum(i => i.Quantity))).ToList();
            if (lines.All(i => remaining.GetValueOrDefault(i.Id) >= i.Quantity)) ready++;
            foreach (var item in lines) remaining[item.Id] = Math.Max(0, remaining.GetValueOrDefault(item.Id) - item.Quantity);
        }
        return new InventoryAreaDto
        {
            Skus = await backoffice.GetSkusAsync(),
            Batches = batches,
            OrderQuota = new OrderQuotaDto
            {
                QuotaDate = date.ToDateTime(TimeOnly.MinValue), TotalOrders = quota.Orders.Count, OrdersReadyToFulfill = ready,
                CurrentQuota = target, BakedToday = baked, RemainingToBake = Math.Max(0, target - baked),
                DailyQuotaPercentage = target == 0 ? 0 : Math.Min(100, 100d * baked / target)
            },
            InventoryQuota = quota.Items.Select(i => new InventoryQuotaItemDto
            {
                QuantityRequired = i.Quantity, QuantityReadyForFulfillment = stock.GetValueOrDefault(i.ProductId),
                Product = new() { ProductId = i.ProductId, ProductName = i.ProductName, ProductImageUrl = i.ProductImageUrl },
                Status = stock.GetValueOrDefault(i.ProductId) >= i.Quantity ? InventoryQuotaItemStatus.ReadyForFulfillment : stock.GetValueOrDefault(i.ProductId) > 0 ? InventoryQuotaItemStatus.InProgress : InventoryQuotaItemStatus.NotStarted
            }).ToList()
        };
    }

    public async Task SaveBatchAsync(BatchDto batch)
    {
        await access.RequireAdminAsync();
        if (batch.Product.Id == Guid.Empty || batch.PlannedQuantity < 1 || batch.ActualQuantity < 0 || batch.DefectiveQuantity < 0 || batch.DefectiveQuantity > batch.ActualQuantity)
            throw new InvalidOperationException("Check the product, planned, actual and defective quantities.");
        if (batch.BatchStatus == BatchStatus.Completed && batch.BatchRuns.Any(r => r.Status != BatchRunStatus.Completed && r.Status != BatchRunStatus.Cancelled))
            throw new InvalidOperationException("Complete the production runs before completing the batch.");
        if (batch.Id == Guid.Empty) batch.Id = Guid.CreateVersion7();
        var settings = await store.LoadAsync<CommerceSettings>("store") ?? new();
        foreach (var run in batch.BatchRuns)
        {
            run.BakingInformation ??= new();
            if (run.BakingInformation.TemperatureInCelsius == 0) run.BakingInformation.TemperatureInCelsius = settings.Preference("defaultBakingTemp", 180);
            if (run.BakingInformation.BakingDurationInMinutes == 0) run.BakingInformation.BakingDurationInMinutes = settings.Preference("defaultBakingDuration", 12);
        }
        await store.ExecuteAsync(["inventory", $"batch:{batch.Id}"], async tx =>
        {
            await tx.SaveAsync(batch.Id.ToString(), batch);
            if (settings.Preference("notifyOnHighDefects", true) && batch.ActualQuantity > 0 && 100m * batch.DefectiveQuantity / batch.ActualQuantity > settings.Preference("maxDefectRatePercent", 5))
                await CommerceAutomation.QueueAsync(tx, settings, $"defects:{batch.Id}", "High batch defect rate", $"Batch {batch.BatchReference}: {batch.DefectiveQuantity} defects out of {batch.ActualQuantity} units.");
            return true;
        });
    }

    public async Task SaveRunAsync(BatchRunDto run)
    {
        await access.RequireAdminAsync();
        if (run.BatchId is null || run.BatchId == Guid.Empty) throw new InvalidOperationException("Assign the run to a batch.");
        await store.ExecuteAsync(["inventory", $"batch:{run.BatchId}"], async tx =>
        {
            var batch = await tx.LoadAsync<BatchDto>(run.BatchId.ToString()!) ?? throw new KeyNotFoundException("Batch not found.");
            var existing = batch.BatchRuns.FirstOrDefault(r => r.Id == run.Id);
            if (existing is not null) batch.BatchRuns.Remove(existing);
            batch.BatchRuns.Add(run);
            await tx.SaveAsync(batch.Id.ToString(), batch);
            return true;
        });
    }
}
