using CookieCommerce.Web.Models.Content;

namespace CookieCommerce.Web.Services;

public interface IContentService
{
    Task<IReadOnlyList<FaqCategoryItemDto>> SearchAsync(string value);
    Task<IReadOnlyList<FaqCategoryDto>> GetFaqsAsync();
}
