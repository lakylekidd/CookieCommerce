namespace CookieCommerce.Web.Models.Products;

public class BatchRunDto
{
    public Guid Id { get; init; }
    public DateTime BakedOn { get; init; }
    public int Quantity { get; init; }
    public int WastedQuantity { get; init; }
    public string BakerName { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}
