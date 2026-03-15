namespace CookieCommerce.Web.Models.Products;

public class BatchDto
{
    public Guid Id { get; init; }
    public DateTime MixedOn { get; init; }
    public string BatchNumber { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public int PlannedQuantity { get; init; }
    public string BakerName { get; init; } = string.Empty;
    public ICollection<BatchRunDto> Runs { get; init; } = [];
    public int TotalBaked => Runs.Sum(r => r.Quantity);
    public int RemainingToBake => PlannedQuantity - TotalBaked;
    public double ProgressPercentage => PlannedQuantity > 0
        ? (double)TotalBaked / PlannedQuantity * 100
        : 0;
}
