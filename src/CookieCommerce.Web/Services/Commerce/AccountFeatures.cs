using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;
using CookieCommerce.Web.Services.Products;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class AccountFeatures(ICommerceStore store, CommerceAccess access, IAuthService auth, ICookieService catalog, CustomerProfileService profiles, ProductMainImages? mainImages = null)
{
    public async Task AddToFavoritesAsync(Guid productId)
    {
        var id = await access.RequireCustomerAsync();
        if (await catalog.GetProductByIdAsync(productId) is null) throw new InvalidOperationException("Product unavailable.");
        await ChangeFavoritesAsync(id, f => f.ProductIds.Add(productId));
    }

    public async Task RemoveFromFavoritesAsync(Guid productId)
    {
        var id = await access.RequireCustomerAsync();
        await ChangeFavoritesAsync(id, f => f.ProductIds.Remove(productId));
    }

    private Task ChangeFavoritesAsync(Guid id, Action<CustomerFavorites> action) => store.ExecuteAsync($"favorites:{id}", async tx =>
    {
        var favorites = await tx.LoadAsync<CustomerFavorites>(id.ToString()) ?? new();
        action(favorites);
        await tx.SaveAsync(id.ToString(), favorites);
        return true;
    });

    public async Task<bool> IsFavoriteAsync(Guid productId)
    {
        var id = await access.CustomerAsync();
        return id is not null && (await store.LoadAsync<CustomerFavorites>(id.ToString()!))?.ProductIds.Contains(productId) == true;
    }

    public async Task<ICollection<StorefrontProductDto>> GetFavoriteProductsAsync()
    {
        var id = await access.CustomerAsync();
        if (id is null) return [];
        var favorites = await store.LoadAsync<CustomerFavorites>(id.ToString()!) ?? new();
        var result = new List<StorefrontProductDto>();
        foreach (var productId in favorites.ProductIds)
            if (await catalog.GetProductByIdAsync(productId) is { } product) result.Add(product);
        return result;
    }

    public Task<UserProfileDto?> GetProfileAsync() => profiles.GetProfileAsync();
    public Task<ResultDto> UpdateProfileAsync(UserProfileDto profile) => profiles.UpdateProfileAsync(profile);
    public async Task<bool> HasBirthdateAsync() => (await profiles.GetProfileAsync())?.DateOfBirth is not null;

    public async Task<List<CommerceOrder>> GetCustomerOrdersAsync()
    {
        var id = await access.CustomerAsync();
        return id is null ? [] : (await store.ListAsync<CommerceOrder>()).Where(o => o.CustomerId == id).OrderByDescending(o => o.Order.PlacedOn).ToList();
    }

    public async Task<ICollection<OrderListItemDto>> GetOrdersAsync(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100) throw new ArgumentOutOfRangeException(nameof(page));
        var orders = (await GetCustomerOrdersAsync()).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        if (mainImages is not null) await mainImages.ApplyAsync(orders.SelectMany(o => o.Order.Products));
        return orders.Select(o => ToListItem(o.Order, o.PaymentStatus == "Paid" ? o.RewardPointsEarned : 0)).ToList();
    }

    public async Task<int> GetTotalOrdersCountAsync() => (await GetCustomerOrdersAsync()).Count;
    public async Task<int> GetOrderCountInNeedOfAttentionAsync() => (await GetCustomerOrdersAsync()).Count(o => o.PaymentStatus == "Pending");

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var id = await access.CustomerAsync();
        if (id is null) return null;
        var order = await store.LoadAsync<CommerceOrder>(orderId.ToString());
        if (order?.CustomerId != id) return null;
        if (mainImages is not null) await mainImages.ApplyAsync(order.Order.Products);
        return order.Order;
    }

    public async Task<TrackingDto?> GetTrackingAsync(Guid orderId) => (await GetOrderByIdAsync(orderId))?.TrackingInformation;

    public async Task SubmitReviewAsync(Guid productId, ReviewSubmitRequest review)
    {
        var id = await access.RequireCustomerAsync();
        if (review.Rating is < 1 or > 5 || string.IsNullOrWhiteSpace(review.Comment) || review.Comment.Length > 2000)
            throw new InvalidOperationException("Enter a rating from 1 to 5 and a review up to 2000 characters.");
        if (!(await GetCustomerOrdersAsync()).Any(o => o.PaymentStatus == "Paid" && o.Order.Status.StatusId >= 5 && o.Order.Status.StatusId <= 9 && o.Order.Products.Any(p => p.ProductId == productId)))
            throw new InvalidOperationException("You can review products from fulfilled orders.");
        var key = $"{id}:{productId}";
        await store.ExecuteAsync($"review:{key}", async tx =>
        {
            if (await tx.LoadAsync<CustomerReview>(key) is not null) throw new InvalidOperationException("You have already reviewed this product.");
            await tx.SaveAsync(key, new CustomerReview
            {
                CustomerId = id, CreatedAt = DateTimeOffset.UtcNow,
                Review = new ReviewDto { Id = Guid.CreateVersion7(), CookieId = productId, Author = $"{auth.CurrentUser!.FirstName} {auth.CurrentUser.LastName.FirstOrDefault()}.", Content = review.Comment.Trim(), Rating = review.Rating }
            });
            return true;
        });
    }

    public async Task<(bool, int)> HasAlreadyReviewedProductIdAsync(Guid productId)
    {
        var id = await access.CustomerAsync();
        if (id is null) return (false, 0);
        var review = await store.LoadAsync<CustomerReview>($"{id}:{productId}");
        return (review is not null, review?.Review.Rating ?? 0);
    }

    public static OrderListItemDto ToListItem(OrderDto order, int rewardPoints = 0) => new()
    {
        Id = order.Id, OrderReference = order.OrderReference, PlacedOn = order.PlacedOn, Total = order.Total,
        Status = order.Status, CurrencySymbol = order.CurrencySymbol, RewardPointsEarned = rewardPoints,
        Products = order.Products.Select(p => new OrderListItemProductDto { ProductName = p.ProductName, ProductImageUrl = p.ProductImageUrl, Quantity = p.Quantity }).ToList()
    };
}
