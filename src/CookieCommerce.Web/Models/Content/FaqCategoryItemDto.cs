using Microsoft.Extensions.AI;

namespace CookieCommerce.Web.Models.Content;

public class FaqCategoryItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Embedding? Embedding { get; set; }
}
