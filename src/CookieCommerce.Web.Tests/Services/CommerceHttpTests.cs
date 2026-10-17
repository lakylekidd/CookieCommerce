using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Services.Payments;
using CookieCommerce.Web.Tests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CookieCommerce.Web.Tests.Services;

public sealed class CommerceHttpTests
{
    [Theory]
    [InlineData("/storefront")]
    [InlineData("/")]
    [InlineData("/shop")]
    [InlineData("/shop/cookie/11111111-1111-1111-1111-111111111111")]
    [InlineData("/shop/checkout")]
    [InlineData("/backoffice/products")]
    [InlineData("/auth/signup")]
    [InlineData("/auth/password/reset/expired-token")]
    public async Task Anonymous_pages_are_hidden_while_coming_soon_mode_is_enabled(string route)
    {
        await using var factory = new SiteFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var response = await client.GetAsync(route);
        var html = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, html);
        html.Should().Contain("COMING SOON");
    }

    [Fact]
    public async Task Signin_remains_available_while_coming_soon_mode_is_enabled()
    {
        await using var factory = new SiteFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var html = await client.GetStringAsync("/auth/signin");

        html.Should().Contain("Welcome Back");
        html.Should().NotContain("Create an Account");
    }

    [Fact]
    public async Task Storefront_is_public_when_coming_soon_mode_is_disabled()
    {
        await using var factory = new SiteFactory();
        await factory.Store.SaveAsync("store", new CommerceSettings { ComingSoonEnabled = false });
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var html = await client.GetStringAsync("/shop");

        html.Should().NotContain("COMING SOON");
        html.Should().Contain("Cookie");
    }

    [Fact]
    public async Task Sitemap_lists_live_products_and_robots_uses_the_same_origin()
    {
        await using var factory = new SiteFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var xml = XDocument.Parse(await client.GetStringAsync("/sitemap.xml"));
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        xml.Descendants(ns + "loc").Select(e => e.Value).Should().Contain("https://localhost/shop/cookie/11111111-1111-1111-1111-111111111111");
        (await client.GetStringAsync("/robots.txt")).Should().Contain("Sitemap: https://localhost/sitemap.xml");
    }

    [Fact]
    public async Task Stripe_endpoint_rejects_unsigned_requests_and_commits_a_verified_payment()
    {
        await using var factory = new SiteFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var id = Guid.CreateVersion7();
        await factory.Store.SaveAsync("guest", new StoredCart { PendingOrderId = id, Cart = new() { CartId = Guid.CreateVersion7(), Items = [new() { Quantity = 1 }] } });
        await factory.Store.SaveAsync(id.ToString(), new CommerceOrder { Id = id, CartKey = "guest", PaymentProvider = "Stripe", PaymentSessionId = "cs_test", Order = new() { Id = id, Total = 10.95m } });
        var payload = JsonSerializer.Serialize(new { type = "checkout.session.completed", data = new { @object = new { id = "cs_test", payment_status = "paid", amount_total = 1095, currency = "eur", metadata = new { order_id = id } } } });
        using var unsigned = await client.PostAsync("/api/payments/stripe/webhook", new StringContent(payload, Encoding.UTF8, "application/json"));
        unsigned.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("test-secret"), Encoding.UTF8.GetBytes($"{timestamp}.{payload}")));
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/stripe/webhook") { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
        request.Headers.Add("Stripe-Signature", $"t={timestamp},v1={signature}");
        using var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await factory.Store.LoadAsync<CommerceOrder>(id.ToString()))!.PaymentStatus.Should().Be("Paid");
        (await factory.Store.LoadAsync<StoredCart>("guest"))!.Cart.Items.Should().BeEmpty();
    }

    private sealed class SiteFactory : WebApplicationFactory<Program>
    {
        public MemoryCommerceStore Store { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("DevelopmentBootstrap:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICommerceStore>();
                // Test storage replaces the sole database schema initializer; remaining app services are real.
                var initializer = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService) && d.ImplementationFactory is not null);
                if (initializer is not null) services.Remove(initializer);
                services.AddSingleton<ICommerceStore>(Store);
                services.Configure<PaymentOptions>(o => o.StripeWebhookSecret = "test-secret");
                services.AddHttpClient("product-domain-api").ConfigurePrimaryHttpMessageHandler(() => new CatalogHandler());
            });
        }
    }

    private sealed class CatalogHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Exercise the intermediate render while the catalog HTTP request is in flight.
            await Task.Delay(10, cancellationToken);
            var product = new { id = Guid.Parse("11111111-1111-1111-1111-111111111111"), name = "Cookie", description = "Cookie", price = 3, isActive = true,
                variants = new[] { new { id = Guid.Parse("22222222-2222-2222-2222-222222222222"), name = "Single", price = 3, currencySymbol = "€", isActive = true, photos = Array.Empty<object>() } } };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = request.RequestUri!.AbsolutePath.TrimEnd('/') == "/api/product" ? JsonContent.Create(new[] { product }) : JsonContent.Create(product)
            };
        }
    }
}
