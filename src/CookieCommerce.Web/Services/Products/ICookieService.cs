using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Products;

public interface ICookieService
{
    Task<IReadOnlyList<FeaturedCookieDto>> GetFeaturedCookiesAsync(CancellationToken cancellationToken = default);
}
