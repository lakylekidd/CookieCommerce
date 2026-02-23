using CookieCommerce.Web.Models.Content;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeContentService : IContentService, IDisposable
{
    private readonly SmartComponents.LocalEmbeddings.LocalEmbedder _embedder = new();
    private List<(FaqCategoryItemDto Item, SmartComponents.LocalEmbeddings.EmbeddingF32 Embedding)> _vectorizedFaqs = new();

    public FakeContentService()
    {
        InitializeEmbeddings();
    }

    public void Dispose()
    {
        _embedder.Dispose();
    }

    public Task<IReadOnlyList<FaqCategoryDto>> GetFaqsAsync()
    {
        var faq = StaticFaqList.FaqData.ToList();
        return Task.FromResult<IReadOnlyList<FaqCategoryDto>>(faq);
    }

    private const float MinSimilarityThreshold = 0.55f; // The threshold for considering a
                                                        // FAQ item relevant to the search query. Adjust as needed.

    public Task<IReadOnlyList<FaqCategoryItemDto>> SearchAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Task.FromResult<IReadOnlyList<FaqCategoryItemDto>>(new List<FaqCategoryItemDto>());

        var userQueryEmbedding = _embedder.Embed(value);

        var results = _vectorizedFaqs
            .Select(f => (f.Item, Score: SmartComponents.LocalEmbeddings.LocalEmbedder.Similarity(userQueryEmbedding, f.Embedding)))
            .Where(x => x.Score >= MinSimilarityThreshold)
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => x.Item)
            .ToList();

        return Task.FromResult<IReadOnlyList<FaqCategoryItemDto>>(results);
    }

    private void InitializeEmbeddings()
    {
        var allItems = StaticFaqList.FaqData.SelectMany(x => x.Items).ToList();

        foreach (var item in allItems)
        {
            // We combine Title and Content so the AI understands the whole context
            string fullText = $"{item.Description} {item.Content}";
            _vectorizedFaqs.Add((item, _embedder.Embed(fullText)));
        }
    }
}
