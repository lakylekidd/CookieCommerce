using System.Net;
using System.Net.Mail;
using System.Text.Json;
using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.DevBootstrap;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class CommerceEmail
{
    public bool Migrated { get; set; }
    public string Key { get; set; } = "";
    public string To { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset RetryAfter { get; set; }
}

public sealed class CustomerNotificationState
{
    public HashSet<Guid> CustomerIds { get; set; } = [];
}

/// <summary>Durable operational mail and production scheduling. Email failures remain queued.</summary>
public sealed class CommerceAutomation(ICommerceStore store, IHttpClientFactory clients, IConfiguration configuration, ILogger<CommerceAutomation> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ScanAsync();
                await MigrateEmailsAsync();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogWarning("Commerce automation will retry on the next scan [failureType={FailureType}]", ex.GetType().Name); }
        }
    }

    public async Task ScanAsync()
    {
        var settings = await store.LoadAsync<CommerceSettings>("store") ?? new();
        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Amsterdam");
        var today = DateOnly.FromDateTime(now.DateTime);
        await store.ExecuteAsync("inventory", async tx =>
        {
            var orders = (await tx.ListAsync<CommerceOrder>()).Where(o => o.PaymentStatus == "Paid" && !o.IsDevelopmentSeed).ToList();
            var batches = (await tx.ListAsync<BatchDto>()).ToList();
            var threshold = Math.Max(0, settings.Preference("lowStockThreshold", 10));
            foreach (var day in orders.Where(o => o.DeliveryDate >= today).GroupBy(o => o.DeliveryDate))
            {
                var lines = day.SelectMany(o => o.Order.Products).ToList();
                var totalRequired = lines.Sum(p => p.Quantity * p.UnitsPerItem);
                foreach (var product in lines.GroupBy(p => p.ProductId))
                {
                    var required = product.Sum(p => p.Quantity * p.UnitsPerItem);
                    var matching = batches.Where(b => b.DeliveryDate == day.Key && b.Product.Id == product.Key && b.BatchStatus != BatchStatus.Cancelled).ToList();
                    var ready = matching.Where(b => b.BatchStatus == BatchStatus.Completed).Sum(b => b.ActualQuantity - b.DefectiveQuantity);
                    if (ready - required >= threshold) continue;
                    if (settings.Preference("notifyOnLowStock", true))
                        await QueueAsync(tx, settings, $"stock:{day.Key}:{product.Key}", "Low stock", $"{product.First().ProductName}: {required} units required for {day.Key:d}; {ready} ready.");
                    if (!settings.Preference("autoCreateBatchOnLowStock", false)) continue;
                    var planned = matching.Where(b => b.BatchStatus != BatchStatus.Completed).Sum(b => b.PlannedQuantity);
                    var dailyTarget = Math.Max(totalRequired, settings.Preference("dailyQuotaTarget", 50));
                    var target = Math.Max(required + threshold, (int)Math.Ceiling((decimal)dailyTarget * required / Math.Max(1, totalRequired)));
                    var shortage = target - ready - planned;
                    if (shortage <= 0) continue;
                    var id = Guid.CreateVersion7();
                    var batch = new BatchDto
                    {
                        Id = id, BatchReference = $"AUTO-{id.ToString("N")[..12]}", DeliveryDate = day.Key,
                        CreatedOn = DateTime.UtcNow, PlannedQuantity = shortage, BatchStatus = BatchStatus.Scheduled,
                        Product = new() { Id = product.Key, Reference = product.Key.ToString(), Name = product.First().ProductName, ImageUrl = product.First().ProductImageUrl },
                        Remarks = "Automatically scheduled to cover paid orders and the configured production target."
                    };
                    await tx.SaveAsync(id.ToString(), batch);
                    batches.Add(batch);
                }
            }
            if (settings.Preference("dailySummaryReport", true) && now.Hour >= 18)
            {
                var daily = orders.Where(o => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(o.Order.PlacedOn, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam"))) == today).ToList();
                await QueueAsync(tx, settings, $"summary:{today}", $"Daily order summary {today:d}", $"{daily.Count} paid orders; revenue EUR {daily.Sum(o => o.Order.Total):F2}.");
            }
            return true;
        });
        await ScanCustomersAsync(settings);
    }

    private async Task ScanCustomersAsync(CommerceSettings settings)
    {
        if (!settings.Preference("notifyOnNewCustomer", false)) return;
        using var response = await clients.CreateClient("customer-domain-api").GetAsync("api/customer");
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var seedEmails = DevelopmentBootstrapData.Customers.Select(c => c.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var customers = json.RootElement.EnumerateArray().Where(c => !seedEmails.Contains(c.GetProperty("email").GetString() ?? ""))
            .Select(c => (Id: c.GetProperty("id").GetGuid(), Name: $"{c.GetProperty("firstName").GetString()} {c.GetProperty("lastName").GetString()}" )).ToList();
        await store.ExecuteAsync("customer-notifications", async tx =>
        {
            var previous = await tx.LoadAsync<CustomerNotificationState>("customers");
            if (previous is not null)
                foreach (var customer in customers.Where(c => !previous.CustomerIds.Contains(c.Id)))
                    await QueueAsync(tx, settings, $"customer:{customer.Id}", "New customer", $"{customer.Name} registered an account.");
            await tx.SaveAsync("customers", new CustomerNotificationState { CustomerIds = customers.Select(c => c.Id).ToHashSet() });
            return true;
        });
    }

    public static async Task QueueAsync(ICommerceTransaction tx, CommerceSettings settings, string key, string subject, string body)
    {
        if (await tx.LoadAsync<CommerceEmail>(key) is not null) return;
        var to = settings.Preference("adminEmail", "admin@tigervancookie.nl");
        if (!MailAddress.TryCreate(to, out _)) return;
        await tx.SaveAsync(key, new CommerceEmail { Key = key, To = to, Subject = subject, Body = body });
    }

    private async Task MigrateEmailsAsync()
    {
        foreach (var pending in (await store.ListAsync<CommerceEmail>()).Where(m => m.SentAt is null && !m.Migrated).Take(20))
        {
            await store.ExecuteAsync($"email:{pending.Key}", async tx =>
            {
                var message = await tx.LoadAsync<CommerceEmail>(pending.Key);
                if (message is null || message.SentAt is not null || message.Migrated) return true;
                var id = "operational:" + message.Key;
                if (await tx.LoadAsync<CookieCommerce.Web.Services.Communications.OutboundMessage>(id) is null)
                    await tx.SaveAsync(id, new CookieCommerce.Web.Services.Communications.OutboundMessage { Id = id, To = message.To, Subject = message.Subject, TextBody = message.Body });
                message.Migrated = true;
                await tx.SaveAsync(message.Key, message);
                return true;
            });
        }
    }
}
