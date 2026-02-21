using CookieCommerce.Web.Models.Content;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeContentService : IContentService
{
    public Task<IReadOnlyList<FaqCategoryDto>> GetFaqsAsync()
    {
        var faq = StaticFaqList.FaqData.ToList();
        return Task.FromResult<IReadOnlyList<FaqCategoryDto>>(faq);
    }

    public Task<IReadOnlyList<FaqCategoryItemDto>> SearchAsync(string value)
    {
        var faq = StaticFaqList.FaqData.SelectMany(x => x.Items)
            .Where(x => x.Description.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                        x.Content.Contains(value, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult<IReadOnlyList<FaqCategoryItemDto>>(faq);
    }
}
