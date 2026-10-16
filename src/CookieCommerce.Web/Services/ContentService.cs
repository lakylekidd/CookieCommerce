using CookieCommerce.Web.Models.Content;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

/// <summary>Editorial FAQ content; static copy is not a simulated commerce service.</summary>
public sealed class ContentService : IContentService
{
    public Task<IReadOnlyList<FaqCategoryDto>> GetFaqsAsync() => Task.FromResult<IReadOnlyList<FaqCategoryDto>>(StaticFaqList.FaqData.ToList());
    public Task<IReadOnlyList<FaqCategoryItemDto>> SearchAsync(string value)
    {
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var results = words.Length == 0 ? [] : StaticFaqList.FaqData.SelectMany(c => c.Items)
            .Select(item => (Item: item, Score: words.Count(w => $"{item.Description} {item.Content}".Contains(w, StringComparison.OrdinalIgnoreCase))))
            .Where(x => x.Score > 0).OrderByDescending(x => x.Score).Take(5).Select(x => x.Item).ToList();
        return Task.FromResult<IReadOnlyList<FaqCategoryItemDto>>(results);
    }
}
