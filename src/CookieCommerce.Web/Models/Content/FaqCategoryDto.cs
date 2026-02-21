namespace CookieCommerce.Web.Models.Content;

public class FaqCategoryDto
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<FaqCategoryItemDto> Items { get; set; } = [];
}
