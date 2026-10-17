using System.Net;
using System.Net.Http.Json;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Models.Backoffice;
using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Tests.Fixtures;
using FluentAssertions;
using Moq;

namespace CookieCommerce.Web.Tests.Services;

public sealed class ProductMainImageTests
{
    private readonly CatalogHandler handler = new();
    private readonly Mock<IHttpClientFactory> factory = new();
    private readonly ProductMainImages images;

    public ProductMainImageTests()
    {
        factory.Setup(f => f.CreateClient("product-domain-api")).Returns(() => new HttpClient(handler, false) { BaseAddress = new Uri("http://catalog/") });
        images = new(new ProductApiClient(factory.Object));
    }

    [Fact]
    public async Task Main_images_replace_variant_thumbnails_and_follow_main_image_changes_even_for_inactive_products()
    {
        var line = new OrderProductDto { ProductId = handler.Id, ProductImageUrl = "/variant.png" };
        var cart = new CartItemDto { ProductId = handler.Id, ProductImageUrl = "/variant.png" };
        var batch = new BatchProductDto { Id = handler.Id, ImageUrl = "/old-main.png" };
        await images.ApplyAsync([line]);
        await images.ApplyAsync([cart]);
        await images.ApplyAsync([batch]);
        line.ProductImageUrl.Should().Be("/main.png");
        cart.ProductImageUrl.Should().Be("/main.png");
        batch.ImageUrl.Should().Be("/main.png");
        handler.MainUrl = "/replacement-main.png";
        await images.ApplyAsync([line]);
        line.ProductImageUrl.Should().Be("/replacement-main.png");
    }

    [Fact]
    public async Task A_missing_main_image_uses_the_placeholder_rather_than_a_variant_photo()
    {
        handler.MainUrl = null;
        var line = new OrderProductDto { ProductId = handler.Id, ProductImageUrl = "/variant.png" };
        await images.ApplyAsync([line]);
        line.ProductImageUrl.Should().Be(ProductMainImages.Fallback);
    }

    [Fact]
    public async Task Existing_customer_history_order_details_and_backoffice_views_use_the_current_main_image()
    {
        var store = new MemoryCommerceStore();
        var user = new UserDto { Id = Guid.CreateVersion7() };
        var auth = new Mock<IAuthService>();
        auth.Setup(a => a.InitializeAsync()).Returns(Task.CompletedTask);
        auth.Setup(a => a.CurrentUser).Returns(user);
        auth.Setup(a => a.IsAdmin).Returns(true);
        var id = Guid.CreateVersion7();
        var today = DateOnly.FromDateTime(DateTime.Today);
        await store.SaveAsync(id.ToString(), new CommerceOrder { Id = id, CustomerId = user.Id, PaymentStatus = "Paid", DeliveryDate = today,
            Order = new() { Id = id, Products = [new() { ProductId = handler.Id, ProductImageUrl = "/old-variant.png", Quantity = 2, UnitPrice = 7 }] } });
        var access = new CommerceAccess(auth.Object);
        var account = new AccountFeatures(store, access, auth.Object, null!, null!, images);
        (await account.GetOrdersAsync(1, 10)).Single().Products.Single().ProductImageUrl.Should().Be("/main.png");
        (await account.GetOrderByIdAsync(id))!.Products.Single().ProductImageUrl.Should().Be("/main.png");
        var backoffice = new BackofficeService(null!, store, access, null, images);
        (await backoffice.GetQuotaDateOrdersAsync(today.ToDateTime(TimeOnly.MinValue))).Orders.Single().Items.Single().ProductImageUrl.Should().Be("/main.png");
        var customers = new CustomerService(factory.Object, store, access, images);
        (await customers.GetCustomerOrdersAsync(user.Id)).Single().Products.Single().ProductImageUrl.Should().Be("/main.png");
        // Thumbnail hydration leaves the saved order snapshot and financial fields unchanged.
        (await store.LoadAsync<CommerceOrder>(id.ToString()))!.Order.Products.Single().ProductImageUrl.Should().Be("/old-variant.png");
        (await account.GetOrderByIdAsync(id))!.Products.Single().UnitPrice.Should().Be(7);
    }

    private sealed class CatalogHandler : HttpMessageHandler
    {
        public Guid Id { get; } = Guid.CreateVersion7();
        public string? MainUrl { get; set; } = "/main.png";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new[] { new { id = Id, name = "Cookie", price = 3, isActive = false,
                mainImage = MainUrl is null ? null : new { url = MainUrl },
                variants = new[] { new { id = Guid.CreateVersion7(), name = "Box", price = 3, currencySymbol = "€", isActive = true, photos = new[] { new { url = "/variant.png" } } } } } }) });
    }
}
