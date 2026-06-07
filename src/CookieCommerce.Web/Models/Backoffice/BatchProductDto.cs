namespace CookieCommerce.Web.Models.Backoffice;

public class BatchProductDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
