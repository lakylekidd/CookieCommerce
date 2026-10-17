using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Models.Checkout;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Services.Payments;
using CookieCommerce.Web.Services.Products;
using CookieCommerce.Web.Tests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace CookieCommerce.Web.Tests.Services;

public sealed class CommerceWorkflowTests
{
    private readonly MemoryCommerceStore store = new();
    private readonly Mock<ICookieService> catalog = new();
    private readonly Mock<IAuthService> auth = new();
    private readonly Guid productId = Guid.CreateVersion7();
    private readonly Guid variantId = Guid.CreateVersion7();
    private readonly EphemeralDataProtectionProvider protection = new();

    public CommerceWorkflowTests()
    {
        auth.Setup(a => a.InitializeAsync()).Returns(Task.CompletedTask);
        catalog.Setup(c => c.GetProductByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(new StorefrontProductDto
        { Id = productId, Title = "Cookie", ImageUrl = "/main.png", Variants = [new StorefrontProductVariantDto { Id = variantId, Title = "Single", Price = 3, ImageUrls = ["/variant.png"] }] });
    }
    private CartService Cart(BrowserStorageRuntime? browser = null) => new(store, catalog.Object, new ProtectedLocalStorage(browser ?? new(), protection));

    [Fact]
    public async Task Carts_are_isolated_and_survive_a_new_circuit()
    {
        var browser = new BrowserStorageRuntime();
        var first = Cart(browser);
        await first.AddToCartAsync(productId, variantId, 2);
        (await Cart().GetCartAsync()).Items.Should().BeEmpty();
        (await Cart(browser).GetCartAsync()).Items.Should().ContainSingle().Which.Quantity.Should().Be(2);
        (await first.GetCartAsync()).Items.Single().ProductImageUrl.Should().Be("/main.png");
        (await first.GetCartItemCountAsync()).Should().Be(2);
        await first.UpdateQuantityAsync((await first.GetCartAsync()).Items.Single().CartItemId, 0);
        (await first.GetCartAsync()).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Signing_in_merges_once_and_logging_out_keeps_customer_items_private()
    {
        var browser = new BrowserStorageRuntime();
        var customer = Guid.CreateVersion7();
        var cart = Cart(browser);
        await cart.AddToCartAsync(productId, variantId, 2);
        await cart.ConvertToUserCartAsync(customer);
        await cart.ConvertToUserCartAsync(customer);
        (await cart.GetCartAsync()).Items.Single().Quantity.Should().Be(2);
        await cart.ResetIdentityAsync();
        (await cart.GetCartAsync()).Items.Should().BeEmpty();
        await cart.ConvertToUserCartAsync(customer);
        (await cart.GetCartAsync()).Items.Single().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task Switching_accounts_does_not_expose_the_previous_customers_pending_cart()
    {
        var cart = Cart();
        var first = Guid.CreateVersion7();
        await cart.ConvertToUserCartAsync(first);
        await cart.AddToCartAsync(productId, variantId, 2);
        var state = await store.LoadAsync<StoredCart>(await cart.GetKeyAsync());
        state!.PendingOrderId = Guid.CreateVersion7();
        await store.SaveAsync(await cart.GetKeyAsync(), state);
        await cart.ConvertToUserCartAsync(Guid.CreateVersion7());
        (await cart.GetCartAsync()).Items.Should().BeEmpty();
        (await store.LoadAsync<StoredCart>($"customer:{first}"))!.Cart.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Checkout_reprices_and_reuses_the_pending_order()
    {
        var cart = Cart();
        await cart.AddToCartAsync(productId, variantId, 2);
        var stale = await store.LoadAsync<StoredCart>(await cart.GetKeyAsync());
        stale!.Cart.Items.Single().UnitPrice = 0.01m;
        stale.Cart.Items.Single().ProductImageUrl = "/old-variant.png";
        await store.SaveAsync(await cart.GetKeyAsync(), stale);
        var gateway = new Mock<IPaymentGateway>();
        gateway.SetupGet(g => g.Name).Returns("Stripe");
        gateway.Setup(g => g.CreateAsync(It.IsAny<CommerceOrder>())).ReturnsAsync(new PaymentSession("cs_test", "https://checkout.stripe.com/test"));
        var settings = new StoreSettingsService(store, new CommerceAccess(auth.Object));
        var checkout = new CheckoutService(store, cart, catalog.Object, auth.Object, settings, [gateway.Object], Options.Create(new PaymentOptions()), NullLogger<CheckoutService>.Instance);
        var address = new AddressDto { Street = "Street", HouseNumber = "1", City = "Amsterdam", Country = "NL", PostalCode = "1011AB" };
        var request = new GuestCheckoutRequest { CartId = (await cart.GetCartAsync()).CartId.ToString(), FristName = "Jamie", LastName = "Baker", Email = "jamie@example.com", ShippingAddress = address, BillingAddress = address, DeliveryMethod = "Standard delivery" };
        var first = await checkout.CheckoutGuestAsync(request);
        var second = await checkout.CheckoutGuestAsync(request);
        first.Success.Should().BeTrue(first.Message);
        second.OrderId.Should().Be(first.OrderId);
        gateway.Verify(g => g.CreateAsync(It.Is<CommerceOrder>(o => o.Order.Total == 10.95m)), Times.Once);
        (await store.ListAsync<CommerceOrder>()).Should().ContainSingle();
        (await store.ListAsync<CommerceOrder>()).Single().Order.Products.Single().ProductImageUrl.Should().Be("/main.png");

        gateway.Setup(g => g.CancelAsync("cs_test")).ReturnsAsync(false);
        (await checkout.CancelPendingAsync()).Should().BeFalse();
        (await store.LoadAsync<StoredCart>(await cart.GetKeyAsync()))!.PendingOrderId.Should().Be(first.OrderId);
        gateway.Setup(g => g.CancelAsync("cs_test")).ReturnsAsync(true);
        (await checkout.CancelPendingAsync()).Should().BeTrue();
        (await cart.GetCartAsync()).Items.Should().ContainSingle();
        (await store.LoadAsync<CommerceOrder>(first.OrderId.ToString()))!.PaymentStatus.Should().Be("Cancelled");
        var retry = await checkout.CheckoutGuestAsync(request);
        retry.Success.Should().BeTrue(retry.Message);
        retry.OrderId!.Value.Should().NotBe(first.OrderId!.Value);
        gateway.Verify(g => g.CreateAsync(It.IsAny<CommerceOrder>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Payment_confirmation_is_idempotent_and_rejects_the_wrong_amount()
    {
        var cartKey = "guest";
        var orderId = Guid.CreateVersion7();
        var cartId = Guid.CreateVersion7();
        await store.SaveAsync(cartKey, new StoredCart { Cart = new() { CartId = cartId, Items = [new() { Quantity = 2 }] }, PendingOrderId = orderId });
        await store.SaveAsync(orderId.ToString(), new CommerceOrder { Id = orderId, CartKey = cartKey, CartId = cartId, PaymentProvider = "Stripe", PaymentSessionId = "cs_test", Order = new() { Id = orderId, Total = 10.95m } });
        var processor = new PaymentProcessor(store, NullLogger<PaymentProcessor>.Instance);
        var invalid = () => processor.ApplyAsync("Stripe", new(orderId, "cs_test", "Paid", 1, "eur"));
        await invalid.Should().ThrowAsync<UnauthorizedAccessException>();
        (await store.LoadAsync<StoredCart>(cartKey))!.Cart.Items.Should().NotBeEmpty();
        var notification = new PaymentNotification(orderId, "cs_test", "Paid", 10.95m, "eur");
        await processor.ApplyAsync("Stripe", notification);
        var clearedId = (await store.LoadAsync<StoredCart>(cartKey))!.Cart.CartId;
        await processor.ApplyAsync("Stripe", notification);
        (await store.LoadAsync<StoredCart>(cartKey))!.Cart.CartId.Should().Be(clearedId);
        (await store.LoadAsync<CommerceOrder>(orderId.ToString()))!.Order.Status.StatusId.Should().Be(2);
    }

    [Fact]
    public void Stripe_webhook_signature_rejects_tampering_and_stale_payloads()
    {
        var gateway = new StripePaymentGateway(Mock.Of<IHttpClientFactory>(), Options.Create(new PaymentOptions { StripeWebhookSecret = "secret" }));
        var now = DateTimeOffset.UtcNow;
        var payload = JsonSerializer.Serialize(new { type = "checkout.session.completed", data = new { @object = new { id = "cs_test", payment_status = "paid", amount_total = 1095, currency = "eur", metadata = new { order_id = Guid.CreateVersion7() } } } });
        var signature = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("secret"), Encoding.UTF8.GetBytes($"{now.ToUnixTimeSeconds()}.{payload}")));
        var header = $"t={now.ToUnixTimeSeconds()},v1={signature}";
        gateway.VerifyWebhook(payload, header, now)!.Status.Should().Be("Paid");
        var altered = () => gateway.VerifyWebhook(payload.Replace("1095", "1"), header, now);
        altered.Should().Throw<UnauthorizedAccessException>();
        var stale = () => gateway.VerifyWebhook(payload, header, now.AddMinutes(6));
        stale.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Favorites_orders_and_reviews_are_scoped_to_the_signed_in_customer()
    {
        var user = new UserDto { Id = Guid.CreateVersion7(), FirstName = "Jamie", LastName = "Baker" };
        auth.Setup(a => a.CurrentUser).Returns(() => user);
        var features = new AccountFeatures(store, new CommerceAccess(auth.Object), auth.Object, catalog.Object, null!);
        await features.AddToFavoritesAsync(productId);
        (await features.IsFavoriteAsync(productId)).Should().BeTrue();
        var orderId = Guid.CreateVersion7();
        await store.SaveAsync(orderId.ToString(), new CommerceOrder { Id = orderId, CustomerId = user.Id, PaymentStatus = "Paid", Order = new OrderDto { Id = orderId, Status = new() { StatusId = 9 }, Products = [new() { ProductId = productId }] } });
        await features.SubmitReviewAsync(productId, new ReviewSubmitRequest { Rating = 5, Comment = "Great cookies" });
        var duplicate = () => features.SubmitReviewAsync(productId, new ReviewSubmitRequest { Rating = 4, Comment = "Another" });
        await duplicate.Should().ThrowAsync<InvalidOperationException>();
        user = new UserDto { Id = Guid.CreateVersion7() };
        (await features.IsFavoriteAsync(productId)).Should().BeFalse();
        (await features.GetOrderByIdAsync(orderId)).Should().BeNull();
        var ineligible = () => features.SubmitReviewAsync(productId, new ReviewSubmitRequest { Rating = 5, Comment = "Not purchased" });
        await ineligible.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Newsletter_subscription_is_validated_and_idempotent()
    {
        var service = new NewsletterService(store);
        (await service.SubscribeAsync("bad email", "Jamie")).Success.Should().BeFalse();
        await service.SubscribeAsync("jamie@example.com", "Jamie");
        await service.SubscribeAsync("JAMIE@example.com", "Jamie");
        (await store.ListAsync<NewsletterSubscription>()).Should().ContainSingle();
    }

    [Fact]
    public async Task Rewards_are_valued_at_one_cent_and_cannot_stack_with_promotions()
    {
        var customerId = Guid.CreateVersion7();
        var cart = Cart();
        await cart.ConvertToUserCartAsync(customerId);
        await cart.AddToCartAsync(productId, variantId, 2);
        await store.SaveAsync("earned-order", new CommerceOrder { CustomerId = customerId, PaymentStatus = "Paid", RewardPointsEarned = 250 });
        await store.SaveAsync("store", new CommerceSettings { Promotions = [new() { Code = "PROMO", Discount = 10 }] });
        (await cart.ApplyRewardPointsAsync(100)).Success.Should().BeTrue();
        (await cart.GetCartAsync()).RewardDiscount.Should().Be(1m);
        (await cart.ApplyPromotionCodeAsync("PROMO")).Success.Should().BeFalse();
        await cart.ApplyRewardPointsAsync(0);
        (await cart.ApplyPromotionCodeAsync("PROMO")).Success.Should().BeTrue();
        (await cart.ApplyRewardPointsAsync(100)).Success.Should().BeFalse();
        await cart.RemovePromotionCodeAsync("PROMO");
        (await cart.ApplyRewardPointsAsync(251)).Success.Should().BeFalse();
    }

    [Fact]
    public void Pending_payments_reserve_points_and_cancellation_releases_them()
    {
        var customerId = Guid.CreateVersion7();
        var earned = new CommerceOrder { CustomerId = customerId, PaymentStatus = "Paid", RewardPointsEarned = 250 };
        var pending = new CommerceOrder { CustomerId = customerId, PaymentStatus = "Pending", RewardPointsRedeemed = 100, RewardPointsEarned = 50 };
        LoyaltyRules.Balance([earned, pending], customerId).Should().Be(150);
        pending.PaymentStatus = "Cancelled";
        LoyaltyRules.Balance([earned, pending], customerId).Should().Be(250);
        pending.PaymentStatus = "Paid";
        LoyaltyRules.Balance([earned, pending], customerId).Should().Be(200);
    }

    [Fact]
    public void Reward_earning_honors_the_minimum_and_birthday_multiplier()
    {
        var settings = new CommerceSettings { AdditionalSettings = new() { ["pointsPerEuro"] = "10", ["minOrderToEarnPoints"] = "20", ["doubleBirthdayPoints"] = "true" } };
        LoyaltyRules.Earned(19, settings, true).Should().Be(0);
        LoyaltyRules.Earned(20, settings, false).Should().Be(200);
        LoyaltyRules.Earned(20, settings, true).Should().Be(400);
    }

    [Fact]
    public async Task Production_automation_schedules_a_single_batch_and_deduplicates_alerts()
    {
        await store.SaveAsync("store", new CommerceSettings { AdditionalSettings = new()
        { ["autoCreateBatchOnLowStock"] = "true", ["lowStockThreshold"] = "2", ["dailyQuotaTarget"] = "12", ["dailySummaryReport"] = "false" } });
        await store.SaveAsync("order", new CommerceOrder { PaymentStatus = "Paid", DeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), Order = new()
        { Products = [new() { ProductId = productId, ProductName = "Cookies", Quantity = 2, UnitsPerItem = 6 }] } });
        var automation = new CommerceAutomation(store, Mock.Of<IHttpClientFactory>(), new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(), NullLogger<CommerceAutomation>.Instance);
        await automation.ScanAsync();
        await automation.ScanAsync();
        (await store.ListAsync<CookieCommerce.Web.Models.Backoffice.BatchDto>()).Should().ContainSingle().Which.PlannedQuantity.Should().Be(14);
        (await store.ListAsync<CommerceEmail>()).Should().ContainSingle();
    }

    [Fact]
    public async Task Orders_cannot_be_marked_ready_without_completed_production()
    {
        auth.Setup(a => a.IsAdmin).Returns(true);
        var access = new CommerceAccess(auth.Object);
        var backoffice = new BackofficeService(null!, store, access);
        var id = Guid.CreateVersion7();
        var date = DateOnly.FromDateTime(DateTime.Today);
        await store.SaveAsync(id.ToString(), new CommerceOrder { Id = id, CartKey = "customer", PaymentStatus = "Paid", DeliveryDate = date,
            Order = new() { Id = id, Status = new() { StatusId = 4 }, Products = [new() { ProductId = productId, Quantity = 2, UnitsPerItem = 6 }] } });
        var premature = () => backoffice.SetOrderAsReadyForPickup(id);
        await premature.Should().ThrowAsync<InvalidOperationException>();
        var inventory = new InventoryService(store, access, backoffice);
        await inventory.SaveBatchAsync(new() { Id = Guid.CreateVersion7(), Product = new() { Id = productId }, DeliveryDate = date, PlannedQuantity = 12, ActualQuantity = 12, BatchStatus = CookieCommerce.Web.Models.Backoffice.BatchStatus.Completed });
        await backoffice.SetOrderAsReadyForPickup(id);
        await backoffice.MarkDeliveredAsync(id);
        (await store.LoadAsync<CommerceOrder>(id.ToString()))!.Order.Status.StatusId.Should().Be(9);
    }
}
