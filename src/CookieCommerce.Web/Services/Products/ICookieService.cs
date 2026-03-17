using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;

namespace CookieCommerce.Web.Services.Products;

public interface ICookieService
{
    Task<IReadOnlyList<ProductDto>> GetTopRatedAsync();
    Task<IReadOnlyList<ProductDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetCookieByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDto>> GetReviewsByCookieIdAsync(Guid cookieId, CancellationToken cancellationToken = default);
}
