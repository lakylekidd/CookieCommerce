using System.Text.Json;
using CookieCommerce.DevBootstrap;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.Development;

/// <summary>Waits for the identity/catalog seed and customer integration events before creating orders.</summary>
public sealed class DevelopmentCommerceBootstrapService(IServiceScopeFactory scopes, IHttpClientFactory clients,
    IHostEnvironment environment, IConfiguration configuration, ILogger<DevelopmentCommerceBootstrapService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!environment.IsDevelopment() || configuration.GetValue<bool?>("DevelopmentBootstrap:Enabled") == false) return;
        for (var attempt = 1; attempt <= 60; attempt++)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var expected = DevelopmentBootstrapData.Products.Select(p => p.ProductId).ToHashSet();
                var products = (await scope.ServiceProvider.GetRequiredService<ProductApiClient>().ListAsync(stoppingToken))
                    .Where(p => expected.Contains(p.Id) && p.Variants.Count > 0).OrderBy(p => p.Id).ToList();
                var customers = await LoadCustomersAsync(stoppingToken);
                if (products.Count == expected.Count && customers.Count >= 10)
                {
                    var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Amsterdam").DateTime);
                    await scope.ServiceProvider.GetRequiredService<DevelopmentCommerceSeeder>().SeedAsync(products, customers, today);
                    logger.LogInformation("Development commerce seed complete: {Customers} customers, {Products} products, 100 seed orders; stock restored to 30 cookies / 100 packaging items", customers.Count, products.Count);
                    return;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogWarning(ex, "Development commerce seed waiting for APIs (attempt {Attempt})", attempt); }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        logger.LogError("Development commerce seed did not finish. Check identity bootstrap, customer integration events and product bootstrap logs.");
    }

    private async Task<List<DevelopmentSeedCustomer>> LoadCustomersAsync(CancellationToken cancellationToken)
    {
        using var response = await clients.CreateClient("customer-domain-api").GetAsync("api/customer", cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var records = document.RootElement.EnumerateArray().ToList();
        var result = new List<DevelopmentSeedCustomer>();
        // The stable definition order ensures both existing login users receive seed order history.
        foreach (var definition in DevelopmentBootstrapData.Customers)
        {
            var entry = records.FirstOrDefault(c => c.GetProperty("email").GetString()?.Equals(definition.Email, StringComparison.OrdinalIgnoreCase) == true);
            if (entry.ValueKind != JsonValueKind.Object) continue;
            var firstName = entry.GetProperty("firstName").GetString() ?? definition.FirstName;
            var lastName = entry.GetProperty("lastName").GetString() ?? definition.LastName;
            AddressBookItemDto? address = null;
            if (entry.TryGetProperty("addresses", out var addresses))
            {
                var primary = addresses.EnumerateArray().FirstOrDefault(a => a.TryGetProperty("isDefault", out var flag) && flag.GetBoolean());
                if (primary.ValueKind == JsonValueKind.Object) address = primary.Deserialize<AddressBookItemDto>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            var fallback = definition.Addresses.First();
            address ??= new() { FirstName = firstName, LastName = lastName, Street = fallback.Street, HouseNumber = fallback.HouseNumber,
                HouseNumberAddition = fallback.HouseNumberAddition ?? "", City = fallback.City, PostalCode = fallback.PostalCode,
                Country = fallback.Country, Province = fallback.Province, Phone = fallback.Phone };
            result.Add(new(entry.GetProperty("id").GetGuid(), definition.Email, firstName, lastName, address));
        }
        return result;
    }
}
