using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;

namespace CookieCommerce.Web.Services.Products;

public class CookieService : ICookieService
{
    public Task<IReadOnlyList<StorefrontProductDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default)
    {
        var cookies = StaticStorefrontProductList.Cookies.Where(c => c.IsFeatured).ToList();
        return Task.FromResult<IReadOnlyList<StorefrontProductDto>>(cookies);
    }

    public Task<IReadOnlyList<StorefrontProductDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default)
    {
        var favorites = StaticStorefrontProductList.Cookies.ToList();
        return Task.FromResult<IReadOnlyList<StorefrontProductDto>>(favorites);
    }

    public Task<StorefrontProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cookie = StaticStorefrontProductList.Cookies.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(cookie);
    }

    public async Task<IReadOnlyList<ReviewDto>> GetReviewsByCookieIdAsync(Guid cookieId, CancellationToken cancellationToken = default)
    {
        // Return a list of 3 dummy reviews for the specified cookie ID
        var reviews = new List<ReviewDto>
        {
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                Author = "Alice P.",
                Content = "Delicious cookie! Highly recommend.",
                Rating = 5
            },
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                ProfileImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDfWr3EHJYXTqAzaDP6tzF-bhGQdiDTliZEPua3CQlQ6hA7oIbFNdsAAUBoBxSDOSgCyuwcbnZP5ZRGXSs0dzM9kRFiBO74zAI42P5Y6XLSG3psmdWtWJ9jpaRDBaS6-rrjSyQP9Il1rwrAyht5vA8f5zOL9NG7g79SFthk68bnTVFhrcexXvFf5T5dEISLhEONBap_ycQ92rayku-iLPaGdsisUmu579E6WVYwajiUtZ4gqXbcATtYgnkePOqwCmuixJV5EUxzpA",
                Author = "Bob John",
                Content = "Pretty good, but a bit too sweet for my taste.",
                Rating = 4
            },
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                Author = "Charlie E.",
                Content = "Not my favorite, but still enjoyable.",
                Rating = 3
            },
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                Author = "Leo T.",
                Content = "Absolutely roar-some! The orange zest in the dough is a game changer. I pounced on the 12-pack and they were gone in a day. 🐯",
                Rating = 5
            },
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                Author = "Sarah Jenkins",
                Content = "The best cookies in the jungle. They arrived so fresh, it felt like they were just pulled out of the oven! My kids loved the tiger paw shapes.",
                Rating = 5
            },
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                Author = "Marcus V.",
                Content = "Order by 6 PM and they really were at my door the next day! Super impressive delivery and even better taste. 🐾",
                Rating = 5
            },
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                Author = "Emily 'Claws' Reed",
                Content = "I'm a tough critic, but these are top-tier. Perfectly crunchy on the outside and soft on the inside. Worth every penny for the 12-pack!",
                Rating = 4
            },
            new ReviewDto
            {
                Id = Guid.NewGuid(),
                CookieId = cookieId,
                Author = "BakeLover99",
                Content = "Found my new favorite snack. The packaging is so cute (love the tiger stripes!), and the chocolate chunks are massive. 10/10 would pounce again.",
                Rating = 5
            }
        };
        return reviews;
    }

    public Task<IReadOnlyList<StorefrontProductDto>> GetTopRatedAsync()
    {
        var topRated = StaticStorefrontProductList.Cookies.OrderByDescending(c => c.Rating).Take(4).ToList();
        return Task.FromResult<IReadOnlyList<StorefrontProductDto>>(topRated);
    }
}
