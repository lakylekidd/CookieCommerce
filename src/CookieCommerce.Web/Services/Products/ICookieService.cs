using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;

namespace CookieCommerce.Web.Services.Products;

public interface ICookieService
{
    Task<IReadOnlyList<CookieDto>> GetTopRatedAsync();
    Task<IReadOnlyList<CookieDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CookieDto>> GetFavoriteCookiesAsync(CancellationToken cancellationToken = default);
    Task<CookieDto?> GetCookieByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDto>> GetReviewsByCookieIdAsync(Guid cookieId, CancellationToken cancellationToken = default);
}
