using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.Web.Models.Customers;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class CustomerService(IHttpClientFactory factory, ICommerceStore store, CommerceAccess access, ProductMainImages? mainImages = null,
    CookieCommerce.Web.Services.Communications.AccountAdministrationService? accounts = null) : ICustomerService
{
    public async Task<CustomerDto> CreateCustomerAsync(string firstName, string lastName, string email, bool sendWelcomeEmail)
    {
        await access.RequireAdminAsync();
        firstName = firstName.Trim();
        lastName = lastName.Trim();
        email = email.Trim().ToLowerInvariant();
        if (firstName.Length == 0 || lastName.Length == 0 || !System.Net.Mail.MailAddress.TryCreate(email, out _))
            throw new InvalidOperationException("Enter a first name, last name and valid email address.");

        using var response = await factory.CreateClient("customer-domain-api").PostAsJsonAsync("api/customer", new
        {
            FirstName = firstName, LastName = lastName, Email = email, Gender = 0, DateOfBirth = (DateTime?)null
        });
        response.EnsureSuccessStatusCode();
        var customerId = await response.Content.ReadFromJsonAsync<Guid>();
        if (customerId == Guid.Empty) throw new InvalidOperationException("The customer service did not return a customer id.");
        if (accounts is null) throw new InvalidOperationException("Account administration is unavailable.");
        await accounts.ProvisionAsync(customerId);
        if (sendWelcomeEmail) await accounts.SendInvitationAsync(customerId);
        return new CustomerDto { Id = customerId, FirstName = firstName, LastName = lastName, Email = email, MemberSince = DateTime.UtcNow };
    }

    public async Task<ICollection<CustomerDto>> GetCustomersAsync()
    {
        await access.RequireAdminAsync();
        using var response = await factory.CreateClient("customer-domain-api").GetAsync("api/customer");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var customers = document.RootElement.EnumerateArray().Select(Map).ToList();
        var orders = await store.ListAsync<CommerceOrder>();
        var subscriptions = await store.ListAsync<NewsletterSubscription>();
        foreach (var customer in customers)
        {
            Enrich(customer, orders);
            customer.IsNewsletterSubscribed = subscriptions.Any(s => s.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase));
        }
        return customers;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        await access.RequireAdminAsync();
        using var response = await factory.CreateClient("customer-domain-api").GetAsync($"api/customer/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var customer = Map(json.RootElement);
        Enrich(customer, await store.ListAsync<CommerceOrder>());
        customer.IsNewsletterSubscribed = await store.LoadAsync<NewsletterSubscription>(NewsletterService.Key(customer.Email)) is not null;
        if (accounts is not null)
        {
            try { customer.IsActive = (await accounts.GetAsync(id)).State is "Active" or "Password setup required"; }
            catch (HttpRequestException) { /* The account-access card provides the explicit unavailable status. */ }
        }
        return customer;
    }

    public async Task<ICollection<OrderListItemDto>> GetCustomerOrdersAsync(Guid customerId)
    {
        await access.RequireAdminAsync();
        var orders = (await store.ListAsync<CommerceOrder>()).Where(o => o.CustomerId == customerId).OrderByDescending(o => o.Order.PlacedOn).ToList();
        if (mainImages is not null) await mainImages.ApplyAsync(orders.SelectMany(o => o.Order.Products));
        return orders.Select(o => AccountFeatures.ToListItem(o.Order, o.PaymentStatus == "Paid" ? o.RewardPointsEarned : 0)).ToList();
    }

    private static void Enrich(CustomerDto customer, IReadOnlyList<CommerceOrder> orders)
    {
        var matching = orders.Where(o => o.CustomerId == customer.Id && o.PaymentStatus == "Paid").ToList();
        customer.TotalOrders = matching.Count;
        customer.TotalSpent = matching.Sum(o => o.Order.Total);
        customer.RewardPoints = LoyaltyRules.Balance(orders, customer.Id);
        customer.LastOrderDate = matching.Count == 0 ? null : matching.Max(o => o.Order.PlacedOn);
    }

    private static CustomerDto Map(JsonElement root)
    {
        var customer = new CustomerDto
        {
            Id = root.GetProperty("id").GetGuid(), FirstName = root.GetProperty("firstName").GetString() ?? "",
            LastName = root.GetProperty("lastName").GetString() ?? "", Email = root.GetProperty("email").GetString() ?? "",
            DateOfBirth = root.TryGetProperty("dateOfBirth", out var birth) && birth.ValueKind == JsonValueKind.String ? DateTime.Parse(birth.GetString()!, System.Globalization.CultureInfo.InvariantCulture) : null,
            Gender = root.TryGetProperty("gender", out var gender) && gender.ValueKind != JsonValueKind.Null ? gender.ToString() : null
        };
        if (root.TryGetProperty("createdAtUtc", out var created) && created.ValueKind == JsonValueKind.String)
            customer.MemberSince = created.GetDateTimeOffset().UtcDateTime;
        if (root.TryGetProperty("addresses", out var addresses))
        {
            var address = addresses.EnumerateArray().FirstOrDefault(a => a.TryGetProperty("isDefault", out var primary) && primary.GetBoolean());
            if (address.ValueKind == JsonValueKind.Object)
            {
                customer.PrimaryAddress = address.Deserialize<Models.Addresses.AddressBookItemDto>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
                if (customer.PrimaryAddress is not null && address.TryGetProperty("addressId", out var addressId)) customer.PrimaryAddress.Id = addressId.GetGuid();
                customer.Phone = customer.PrimaryAddress?.Phone;
            }
        }
        return customer;
    }
}
