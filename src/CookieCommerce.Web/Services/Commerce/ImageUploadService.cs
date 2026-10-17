using CookieCommerce.Web.Models.Products;
using Microsoft.AspNetCore.Components.Forms;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class ImageUploadService(IBackofficeService backoffice) : IImageUploadService
{
    private async Task<ProductDto> FindAsync(Guid variantId) => (await backoffice.GetInventoryItemsAsync()).FirstOrDefault(p => p.Variants.Any(v => v.Id == variantId)) ?? throw new KeyNotFoundException("Variant not found.");
    public async Task<ProductImageDto> ProcessImageUploadAsync(IBrowserFile file, Guid variantId)
    {
        var product = await FindAsync(variantId);
        await using var stream = file.OpenReadStream(5 * 1024 * 1024);
        return await backoffice.UploadVariantPhotoAsync(product.ProductId, variantId, new(stream, file.Name, file.ContentType, file.Size));
    }
    public async Task DeletePhotoAsync(Guid imageId, Guid variantId)
    {
        var product = await FindAsync(variantId);
        await backoffice.RemoveVariantPhotoAsync(product.ProductId, variantId, imageId);
    }
    public async Task<List<ProductImageDto>> GetImagesForVariantAsync(Guid variantId) => (await backoffice.GetVariantAsync(variantId)).Images.ToList();
}
