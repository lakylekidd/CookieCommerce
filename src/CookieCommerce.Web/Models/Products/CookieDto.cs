namespace CookieCommerce.Web.Models.Products;

public class CookieDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CurrencySymbol { get; init; } = "$";
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsBestSeller { get; init; }
    public bool IsLimitedEdition { get; init; }
    public bool IsFeatured { get; init; } = false;
    public string[] Ingredients { get; init; } = [];
    public float Rating { get; init; } = 0.0f;
    public int ReviewCount { get; init; } = 0;
}
