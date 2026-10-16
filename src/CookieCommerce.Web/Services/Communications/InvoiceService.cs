using System.Security.Cryptography;
using System.Text.Json;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services.Communications;

public sealed class InvoiceGenerator(ICommerceStore store, InvoiceRenderer renderer)
{
    public async Task<InvoiceRecord> EnsureAsync(Guid orderId)
    {
        var invoice = await store.ExecuteAsync("invoice-issuance", async tx =>
        {
            var existing = await tx.LoadAsync<InvoiceRecord>(orderId.ToString());
            if (existing is not null) return existing;
            var order = await tx.LoadAsync<CommerceOrder>(orderId.ToString()) ?? throw new InvalidOperationException("Order was not found.");
            if (order.PaymentStatus != "Paid") throw new InvalidOperationException("An invoice is issued after payment is confirmed.");
            if (!order.IsDevelopmentSeed && !order.Order.HasSavedTaxRate)
                throw new InvalidOperationException("This historical order has no saved VAT rate. Review its financial data before issuing an invoice.");
            var company = await tx.LoadAsync<InvoiceSettings>("company") ?? new();
            var template = await tx.LoadAsync<MessageTemplate>("invoice-pdf:published") ?? TemplateService.Default("invoice-pdf");
            var year = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Amsterdam").Year.ToString();
            var sequenceKey = order.IsDevelopmentSeed ? "demo:" + year : year;
            var sequence = await tx.LoadAsync<InvoiceSequence>(sequenceKey) ?? new();
            var copy = JsonSerializer.Deserialize<CookieCommerce.Web.Models.Order.OrderDto>(JsonSerializer.Serialize(order.Order))!;
            if (copy.TaxRate is < 0 or > 1 || copy.Total <= 0) throw new InvalidOperationException("The order has incomplete financial information.");
            var tax = decimal.Round(copy.Total * copy.TaxRate / (1 + copy.TaxRate), 2, MidpointRounding.AwayFromZero);
            var issued = new InvoiceRecord
            {
                Id = orderId, Number = $"{(order.IsDevelopmentSeed ? "DEMO-" : "")}{company.Prefix}-{year}-{sequence.Next++:000000}", IssuedAt = DateTimeOffset.UtcNow,
                CustomerId = order.CustomerId, Email = order.Email, Order = copy, Company = company, Template = template,
                TaxAmount = tax, NetAmount = copy.Total - tax, IsDevelopmentSeed = order.IsDevelopmentSeed, LogoBytes = renderer.ReadLogo(company.LogoPath)
            };
            await tx.SaveAsync(sequenceKey, sequence); await tx.SaveAsync(orderId.ToString(), issued);
            return issued;
        });
        if (invoice.State == "Ready") return invoice;
        var bytes = renderer.Render(invoice);
        await store.ExecuteAsync("invoice:" + orderId, async tx =>
        {
            var stored = await tx.LoadAsync<InvoiceRecord>(orderId.ToString()) ?? invoice;
            if (stored.State == "Ready") return true;
            await tx.SaveAsync(orderId.ToString(), new InvoicePdf { Id = orderId, Bytes = bytes });
            stored.State = "Ready"; stored.ContentHash = Convert.ToHexString(SHA256.HashData(bytes));
            await tx.SaveAsync(orderId.ToString(), stored);
            if (stored.Company.EmailInvoices && !stored.IsDevelopmentSeed)
                await QueueEmailAsync(tx, stored, "invoice:" + orderId);
            return true;
        });
        return (await store.LoadAsync<InvoiceRecord>(orderId.ToString()))!;
    }

    public static Task QueueEmailAsync(ICommerceTransaction tx, InvoiceRecord invoice, string key) =>
        OutboundEmailService.QueueTemplateAsync(tx, key, invoice.Email, "invoice-email", Values(invoice), invoice.Id);

    public static Dictionary<string, string> Values(InvoiceRecord invoice) => new()
    {
        ["store_name"] = invoice.Company.CompanyName, ["customer_name"] = $"{invoice.Order.BillingAddress.FirstName} {invoice.Order.BillingAddress.LastName}".Trim(),
        ["invoice_number"] = invoice.Number, ["order_reference"] = invoice.Order.OrderReference, ["order_total"] = "EUR " + invoice.Order.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
    };
}

public sealed class InvoiceService(ICommerceStore store, InvoiceGenerator generator, CommerceAccess access, IAuthService auth)
{
    private async Task AuthorizeAsync(Guid orderId)
    {
        await auth.InitializeAsync();
        if (auth.IsAdmin) { await access.RequireAdminAsync(); return; }
        var owner = await access.RequireCustomerAsync();
        var order = await store.LoadAsync<CommerceOrder>(orderId.ToString());
        if (order?.CustomerId != owner) throw new UnauthorizedAccessException("Invoice is not available for this account.");
    }

    public async Task<(InvoiceRecord Invoice, byte[] Pdf)> DownloadAsync(Guid orderId)
    {
        await AuthorizeAsync(orderId);
        var invoice = await generator.EnsureAsync(orderId);
        return (invoice, (await store.LoadAsync<InvoicePdf>(orderId.ToString()))!.Bytes);
    }

    public async Task SendAsync(Guid orderId)
    {
        await access.RequireAdminAsync();
        var invoice = await generator.EnsureAsync(orderId);
        await store.ExecuteAsync("invoice-mail:" + orderId, async tx =>
        {
            await InvoiceGenerator.QueueEmailAsync(tx, invoice, $"invoice-manual:{orderId}:{Guid.CreateVersion7()}"); return true;
        });
    }

    public async Task<int> BackfillAsync()
    {
        await access.RequireAdminAsync();
        var count = 0;
        foreach (var order in (await store.ListAsync<CommerceOrder>()).Where(o => o.PaymentStatus == "Paid"))
        { await generator.EnsureAsync(order.Id); count++; }
        return count;
    }

    public async Task<InvoiceSettings> SettingsAsync() { await access.RequireAdminAsync(); return await store.LoadAsync<InvoiceSettings>("company") ?? new(); }
    public async Task SaveSettingsAsync(InvoiceSettings settings)
    {
        await access.RequireAdminAsync();
        if (string.IsNullOrWhiteSpace(settings.CompanyName) || !System.Text.RegularExpressions.Regex.IsMatch(settings.Prefix, "^[A-Za-z0-9-]{1,12}$")
            || !settings.LogoPath.StartsWith("/images/", StringComparison.Ordinal) || settings.LogoPath.Contains(".."))
            throw new InvalidOperationException("Enter a company name, valid invoice prefix and an image path under /images/.");
        await store.ExecuteAsync("invoice-company", async tx => { await tx.SaveAsync("company", settings); return true; });
    }
}

public sealed class InvoiceWorker(ICommerceStore store, InvoiceGenerator generator, ILogger<InvoiceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                // Paid orders already present at deployment are handled by the explicit backfill action.
                foreach (var order in (await store.ListAsync<CommerceOrder>()).Where(o => o.InvoiceRequested))
                {
                    try
                    {
                        await generator.EnsureAsync(order.Id);
                        await store.ExecuteAsync(order.CartKey, async tx =>
                        {
                            var current = await tx.LoadAsync<CommerceOrder>(order.Id.ToString());
                            if (current is not null) { current.InvoiceRequested = false; await tx.SaveAsync(order.Id.ToString(), current); }
                            return true;
                        });
                    }
                    catch (Exception ex) when (!ct.IsCancellationRequested) { logger.LogWarning("Invoice {OrderId} will retry ({FailureType})", order.Id, ex.GetType().Name); }
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested) { logger.LogWarning("Invoice worker will retry ({FailureType})", ex.GetType().Name); }
        }
    }
}
