using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Payments;

public sealed class PaymentProcessor(ICommerceStore store, ILogger<PaymentProcessor> logger)
{
    public async Task ApplyAsync(string provider, PaymentNotification notification)
    {
        var saved = await store.LoadAsync<CommerceOrder>(notification.OrderId.ToString())
            ?? throw new InvalidOperationException("Order not found.");
        await store.ExecuteAsync([saved.CartKey, "loyalty"], async tx =>
        {
            var order = await tx.LoadAsync<CommerceOrder>(notification.OrderId.ToString()) ?? throw new InvalidOperationException("Order not found.");
            if (!order.PaymentProvider.Equals(provider, StringComparison.OrdinalIgnoreCase) || order.PaymentSessionId != notification.SessionId)
                throw new InvalidOperationException("Payment session does not match the order.");
            if (notification.Amount != order.Order.Total || !notification.Currency.Equals("EUR", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Payment amount or currency does not match the order.");
            if (order.PaymentStatus == "Paid") return true;
            if (notification.Status == "Pending" && order.PaymentStatus is "Failed" or "Expired" or "Cancelled") return true;
            order.PaymentStatus = notification.Status;
            var cart = await tx.LoadAsync<StoredCart>(order.CartKey);
            if (notification.Status == "Paid")
            {
                order.InvoiceRequested = true;
                order.Order.Status = new OrderStatusDto { StatusId = 2, Status = "Confirmed" };
                order.Order.TrackingInformation.TrackingStatusDescription = "Preparing your order";
                var settings = await tx.LoadAsync<CommerceSettings>("store") ?? new();
                if (!order.IsDevelopmentSeed && System.Net.Mail.MailAddress.TryCreate(order.Email, out _))
                    await CookieCommerce.Web.Services.Communications.OutboundEmailService.QueueTemplateAsync(tx,
                        "confirmation:" + order.Id, order.Email, "order-confirmation", new Dictionary<string, string>
                        {
                            ["customer_name"] = $"{order.Order.BillingAddress.FirstName} {order.Order.BillingAddress.LastName}".Trim(),
                            ["store_name"] = (await tx.LoadAsync<CookieCommerce.Web.Services.Communications.InvoiceSettings>("company"))?.CompanyName ?? "Tiger van Cookie",
                            ["order_reference"] = order.Order.OrderReference, ["order_total"] = "EUR " + order.Order.Total.ToString("F2"), ["invoice_number"] = ""
                        });
                if (settings.Preference("notifyOnNewOrder", true))
                    await CommerceAutomation.QueueAsync(tx, settings, $"order:{order.Id}", $"New paid order {order.Order.OrderReference}", $"Payment confirmed for EUR {order.Order.Total:F2}; delivery date {order.DeliveryDate:d}.");
                if (cart?.PendingOrderId == order.Id)
                    await tx.SaveAsync(order.CartKey, new StoredCart { Cart = new() { CartId = Guid.CreateVersion7(), UserId = cart.Cart.UserId } });
            }
            else if (notification.Status is "Failed" or "Expired" or "Cancelled")
            {
                order.Order.Status = new OrderStatusDto { StatusId = notification.Status == "Failed" ? 0 : 10, Status = notification.Status };
                if (cart?.PendingOrderId == order.Id)
                {
                    cart.PendingOrderId = null;
                    await tx.SaveAsync(order.CartKey, cart);
                }
            }
            await tx.SaveAsync(order.Id.ToString(), order);
            return true;
        });

        logger.LogInformation(
            "Payment notification applied [provider={Provider}, orderId={OrderId}, paymentStatus={PaymentStatus}]",
            provider, notification.OrderId, notification.Status);
    }
}
