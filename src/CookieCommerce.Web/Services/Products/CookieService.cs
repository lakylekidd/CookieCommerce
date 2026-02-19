using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;

namespace CookieCommerce.Web.Services.Products;

public class CookieService : ICookieService
{
    public Task<IReadOnlyList<CookieDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default)
    {
        var cookies = StaticCookieList.Cookies.Where(c => c.IsFeatured).ToList();
        return Task.FromResult<IReadOnlyList<CookieDto>>(cookies);
    }

    public Task<IReadOnlyList<CookieDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default)
    {
        var favorites = StaticCookieList.Cookies.ToList();
        return Task.FromResult<IReadOnlyList<CookieDto>>(favorites);
    }

    public Task<CookieDto?> GetCookieByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cookie = StaticCookieList.Cookies.FirstOrDefault(c => c.Id == id);
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
            }
        };
        return reviews;
    }
}
