namespace CookieCommerce.Web.Models.Reviews;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid CookieId { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
}
