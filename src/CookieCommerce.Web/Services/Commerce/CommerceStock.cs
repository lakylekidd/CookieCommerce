using CookieCommerce.Web.Models.Backoffice;

namespace CookieCommerce.Web.Services.Commerce;

public static class CommerceStock
{
    public static Dictionary<Guid, int> Available(IEnumerable<BatchDto> batches, IEnumerable<CommerceOrder> orders,
        IEnumerable<InventoryStockAdjustment> adjustments, DateOnly? deliveryDate = null)
    {
        var result = batches.Where(b => b.BatchStatus == BatchStatus.Completed && (deliveryDate is null || b.DeliveryDate <= deliveryDate))
            .GroupBy(b => b.Product.Id).ToDictionary(g => g.Key, g => g.Sum(b => b.ActualQuantity - b.DefectiveQuantity));
        foreach (var adjustment in adjustments.Where(a => deliveryDate is null || a.EffectiveDate <= deliveryDate))
            result[adjustment.ProductId] = result.GetValueOrDefault(adjustment.ProductId) + adjustment.Quantity;
        foreach (var line in orders.Where(o => IsFulfilled(o) && (deliveryDate is null || o.DeliveryDate < deliveryDate)).SelectMany(o => o.Order.Products))
            result[line.ProductId] = result.GetValueOrDefault(line.ProductId) - line.Quantity * line.UnitsPerItem;
        return result.ToDictionary(p => p.Key, p => Math.Max(0, p.Value));
    }

    public static bool IsFulfilled(CommerceOrder order) => order.PaymentStatus == "Paid" && order.Order.Status.StatusId is >= 5 and <= 9;
}
