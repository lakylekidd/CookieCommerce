using System.ComponentModel.DataAnnotations;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Models.Cart;
using CookieCommerce.Web.Models.Checkout;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services.Payments;
using CookieCommerce.Web.Services.Products;
using Microsoft.Extensions.Options;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class CheckoutService(
    ICommerceStore store, CartService carts, ICookieService catalog, IAuthService auth,
    StoreSettingsService settings, IEnumerable<IPaymentGateway> gateways,
    IOptions<PaymentOptions> payments, ILogger<CheckoutService> logger, CustomerProfileService? profiles = null) : ICheckoutService
{
    public async Task<IReadOnlyList<DeliveryInfoDto>> GetDeliveryMethodsAsync(AddressBookItemDto address)
    {
        if (string.IsNullOrWhiteSpace(address.Country)) return [];
        var config = await settings.GetAsync();
        var local = address.Country.Equals("The Netherlands", StringComparison.OrdinalIgnoreCase) ||
            address.Country.Equals("Netherlands", StringComparison.OrdinalIgnoreCase) || address.Country.Equals("NL", StringComparison.OrdinalIgnoreCase);
        // Configured shipping rates; no carrier booking is implied by an estimate.
        if (!local) return [];
        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Amsterdam");
        var date = DateOnly.FromDateTime(now.DateTime).AddDays(TimeOnly.FromDateTime(now.DateTime) < config.NextDayDeliveryCutoff ? 1 : 2);
        return [new DeliveryInfoDto { Title = "Standard delivery", Price = config.StandardShippingCost, DeliveryDate = date }];
    }

    public Task<CheckoutResult> CheckoutGuestAsync(GuestCheckoutRequest request) => CheckoutAsync(request, false, false);
    public Task<CheckoutResult> CheckoutNewCustomerAsync(NewCustomerCheckoutRequest request) => CheckoutAsync(request, false, true);
    public Task<CheckoutResult> CheckoutExistingCustomerAsync(ExistingCustomerCheckoutRequest request) => CheckoutAsync(request, true, false);

    public async Task<bool> CancelPendingAsync()
    {
        var key = await carts.GetKeyAsync();
        return await store.ExecuteAsync([key, "loyalty"], async tx =>
        {
            var state = await tx.LoadAsync<StoredCart>(key);
            if (state?.PendingOrderId is not { } id) return true;
            var order = await tx.LoadAsync<CommerceOrder>(id.ToString()) ?? throw new InvalidOperationException("Order not found.");
            if (order.PaymentStatus == "Paid") return false;
            if (order.PaymentSessionId is { } session)
            {
                var gateway = gateways.Single(g => g.Name.Equals(order.PaymentProvider, StringComparison.OrdinalIgnoreCase));
                if (!await gateway.CancelAsync(session)) return false;
            }
            order.PaymentStatus = "Cancelled";
            order.Order.Status = new OrderStatusDto { StatusId = 10, Status = "Cancelled" };
            state.PendingOrderId = null;
            await tx.SaveAsync(key, state);
            await tx.SaveAsync(id.ToString(), order);
            return true;
        });
    }

    private async Task<CheckoutResult> CheckoutAsync(AnonymousCheckoutRequest request, bool existing, bool register)
    {
        try
        {
            Validate(request);
            var config = await settings.GetAsync();
            await auth.InitializeAsync();
            if (existing && (auth.CurrentUser is null || request is not ExistingCustomerCheckoutRequest customer || customer.UserId != auth.CurrentUser.Id.ToString()))
                return Fail("Sign in to place an order for this account.");
            if (!existing && !register && !config.AllowGuestCheckout) return Fail("Please sign in before checking out.");
            if (register && auth.CurrentUser is not null && !auth.CurrentUser.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                return Fail("Sign out or use your signed-in email address.");
            if (register && auth.CurrentUser is null && request is NewCustomerCheckoutRequest signup)
            {
                if (signup.Password != signup.ConfirmPassword || signup.Password.Length < 8) return Fail("Check your password and confirmation.");
                var result = await auth.SignUpAsync(new SignupDto { FirstName = signup.FristName, LastName = signup.LastName, Email = signup.Email, Password = signup.Password });
                if (!result.Success) return Fail(result.ErrorMessage ?? "Unable to create your account.");
                if (!await auth.SignInAsync(signup.Email, signup.Password)) return Fail("Your account was created. Please sign in to complete checkout.");
                request.CartId = (await carts.GetCartAsync()).CartId.ToString();
            }
            var cartKey = await carts.GetKeyAsync();
            var dateOfBirth = config.Preference("doubleBirthdayPoints", false) && auth.CurrentUser is not null && profiles is not null
                ? (await profiles.GetProfileAsync())?.DateOfBirth : null;
            var order = await store.ExecuteAsync([cartKey, "loyalty"], async tx =>
            {
                var state = await tx.LoadAsync<StoredCart>(cartKey) ?? throw new InvalidOperationException("Your cart is empty.");
                if (state.PendingOrderId is { } pending)
                    return await tx.LoadAsync<CommerceOrder>(pending.ToString()) ?? throw new InvalidOperationException("Unable to load the pending order.");
                if (state.Cart.CartId.ToString() != request.CartId) throw new InvalidOperationException("Your cart has changed. Please reload checkout.");
                var cart = state.Cart;
                if (cart.Items.Count == 0 || cart.Items.Any(i => i.Quantity <= 0) || cart.Items.Sum(i => i.Quantity) > config.MaxItemsPerOrder)
                    throw new InvalidOperationException("Check the quantities in your cart.");
                foreach (var item in cart.Items)
                {
                    var product = await catalog.GetProductByIdAsync(item.ProductId) ?? throw new InvalidOperationException($"{item.ProductName} is no longer available.");
                    var variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId) ?? throw new InvalidOperationException($"{item.ProductVariantName} is no longer available.");
                    item.UnitPrice = variant.Price;
                    item.ProductName = product.Title;
                    item.ProductVariantName = variant.Title;
                    item.ProductImageUrl = product.ImageUrl;
                }
                if (cart.Promotion is not null)
                {
                    var promo = config.Promotions.FirstOrDefault(p => p.IsActive && p.Code.Equals(cart.Promotion.Code, StringComparison.OrdinalIgnoreCase));
                    if (promo is null) throw new InvalidOperationException("The promotion is no longer valid. Remove it from your cart to continue.");
                    cart.Promotion = new CartPromotionDto { Code = promo.Code, Description = promo.Description, IsPercentage = promo.IsPercentage, DiscountAmount = promo.IsPercentage ? promo.Discount / 100 : promo.Discount };
                }
                var methods = await GetDeliveryMethodsAsync(ToAddress(request.ShippingAddress, request));
                if (cart.RewardPointsToRedeem > 0)
                {
                    if (auth.CurrentUser is null || cart.UserId != auth.CurrentUser.Id || cart.Promotion is not null ||
                        !config.Preference("allowPointRedemption", true) || cart.RewardPointsToRedeem < Math.Max(1, config.Preference("minPointsToRedeem", 100)) ||
                        cart.RewardPointsToRedeem > LoyaltyRules.Balance(await store.ListAsync<CommerceOrder>(), auth.CurrentUser.Id) ||
                        cart.RewardPointsToRedeem * LoyaltyRules.PointValue > cart.SubTotalPrice)
                        throw new InvalidOperationException("Your reward points cannot be applied. Remove them or update your cart before checking out.");
                }
                var method = methods.FirstOrDefault(m => m.Title == request.DeliveryMethod) ?? throw new InvalidOperationException("Select an available delivery method.");
                cart.FreeShippingThreshold = config.FreeShippingThreshold;
                cart.SelectedShippingCost = method.Price;
                cart.TaxRate = config.VatRate / 100;
                var id = Guid.CreateVersion7();
                var created = new CommerceOrder
                {
                    Id = id, CartKey = cartKey, CartId = cart.CartId,
                    CustomerId = existing || register ? auth.CurrentUser!.Id : null,
                    Email = existing || register ? auth.CurrentUser!.Email : request.Email.Trim(),
                    DeliveryDate = method.DeliveryDate, Note = config.AllowOrderNotes ? cart.Note : "",
                    PaymentProvider = payments.Value.Provider,
                    RewardPointsRedeemed = cart.RewardPointsToRedeem,
                    Order = new OrderDto
                    {
                        Id = id, OrderReference = $"{config.OrderReferencePrefix}-{id.ToString("N")[..12].ToUpperInvariant()}",
                        PlacedOn = DateTime.UtcNow, ShippingAddress = ToAddress(request.ShippingAddress, request),
                        BillingAddress = ToAddress(request.BillingAddress, request), CurrencySymbol = "€",
                        Promotion = cart.Promotion, RewardDiscount = cart.RewardDiscount, ShippingCharge = cart.Shipping, TaxRate = cart.TaxRate,
                        Total = decimal.Round(cart.TotalPrice, 2), Status = new OrderStatusDto { StatusId = 1, Status = "Awaiting payment" },
                        Products = cart.Items.Select(i => new OrderProductDto
                        {
                            ProductId = i.ProductId, ProductVariantId = i.ProductVariantId, ProductVariantName = i.ProductVariantName,
                            ProductName = i.ProductName, ProductImageUrl = i.ProductImageUrl, Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice, CurrencySymbol = "€", IsLimitedEdition = i.IsLimitedEdition
                        }).ToList(),
                        TrackingInformation = new TrackingDto { Carrier = method.Title, TrackingStatusDescription = "Awaiting payment", TrackingNumber = "" }
                    }
                };
                foreach (var line in created.Order.Products)
                {
                    var metadata = await tx.LoadAsync<CatalogMetadata>(line.ProductId.ToString());
                    line.UnitsPerItem = Math.Max(1, metadata?.VariantSkus.GetValueOrDefault(line.ProductVariantId)?.Where(s => s.SkuType == "Product").Sum(s => s.Quantity) ?? 1);
                }
                if (created.Order.Total <= 0) throw new InvalidOperationException("The order total must be greater than zero.");
                var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Europe/Amsterdam").DateTime);
                created.RewardPointsEarned = LoyaltyRules.Earned(created.Order.Total, config, dateOfBirth is { } birthday && birthday.Month == localDate.Month && birthday.Day == localDate.Day);
                state.PendingOrderId = id;
                await tx.SaveAsync(id.ToString(), created);
                await tx.SaveAsync(cartKey, state);
                return created;
            });
            return await store.ExecuteAsync(order.CartKey, async tx =>
            {
                var current = await tx.LoadAsync<CommerceOrder>(order.Id.ToString()) ?? order;
                var state = await tx.LoadAsync<StoredCart>(order.CartKey);
                if (current.PaymentStatus != "Pending" || state?.PendingOrderId != current.Id)
                    return Fail("This checkout is no longer pending. Please reload your cart.");
                if (current.PaymentUrl is null)
                {
                    var gateway = gateways.FirstOrDefault(g => g.Name.Equals(current.PaymentProvider, StringComparison.OrdinalIgnoreCase))
                        ?? throw new InvalidOperationException("The configured payment provider is unavailable.");
                    var session = await gateway.CreateAsync(current);
                    current.PaymentSessionId = session.Id;
                    current.PaymentUrl = session.Url;
                    await tx.SaveAsync(current.Id.ToString(), current);
                }
                return new CheckoutResult { Success = true, OrderId = current.Id, PaymentUrl = current.PaymentUrl };
            });
        }
        catch (InvalidOperationException ex) { return Fail(ex.Message); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to start checkout");
            return Fail("Unable to start checkout right now. Your cart has been kept; please try again.");
        }
    }

    private static CheckoutResult Fail(string message) => new() { Message = message };
    private static void Validate(AnonymousCheckoutRequest request)
    {
        if (!new EmailAddressAttribute().IsValid(request.Email) || string.IsNullOrWhiteSpace(request.FristName) || string.IsNullOrWhiteSpace(request.LastName))
            throw new InvalidOperationException("Enter your name and a valid email address.");
        foreach (var address in new[] { request.ShippingAddress, request.BillingAddress })
            if (address is null || string.IsNullOrWhiteSpace(address.Street) || string.IsNullOrWhiteSpace(address.HouseNumber) ||
                string.IsNullOrWhiteSpace(address.City) || string.IsNullOrWhiteSpace(address.PostalCode) || string.IsNullOrWhiteSpace(address.Country))
                throw new InvalidOperationException("Complete your shipping and billing addresses.");
    }

    private static AddressBookItemDto ToAddress(AddressDto address, AnonymousCheckoutRequest request) => new()
    {
        FirstName = (address.FirstName ?? request.FristName).Trim(), LastName = (address.LastName ?? request.LastName).Trim(), Street = address.Street.Trim(),
        HouseNumber = address.HouseNumber.Trim(), HouseNumberAddition = address.HouseNumberAddition ?? "",
        City = address.City.Trim(), PostalCode = address.PostalCode.Trim(), Province = address.Province ?? "", Country = address.Country.Trim()
    };
}
