namespace CookieCommerce.Web.Models.Products;

public class InventoryItemWithVariantsDto : InventoryItemDto
{
    public ICollection<ProductVariantDto> Variants { get; set; } = [];
}
