using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;

namespace CookieCommerce.Web.Services.Products;

public interface ICookieService
{
    Task<IReadOnlyList<StorefrontProductDto>> GetTopRatedAsync();
    Task<IReadOnlyList<StorefrontProductDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StorefrontProductDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default);
    Task<StorefrontProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDto>> GetReviewsByCookieIdAsync(Guid cookieId, CancellationToken cancellationToken = default);
}
