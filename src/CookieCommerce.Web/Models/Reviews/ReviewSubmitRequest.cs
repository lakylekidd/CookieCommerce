namespace CookieCommerce.Web.Models.Reviews;

public class ReviewSubmitRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
