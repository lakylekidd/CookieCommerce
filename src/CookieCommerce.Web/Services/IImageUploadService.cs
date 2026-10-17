using CookieCommerce.Web.Models.Products;
using Microsoft.AspNetCore.Components.Forms;

namespace CookieCommerce.Web.Services;

public interface IImageUploadService
{
    Task<ProductImageDto> ProcessImageUploadAsync(IBrowserFile file, Guid variantId);
    Task DeletePhotoAsync(Guid imageId, Guid variantId);
    Task<List<ProductImageDto>> GetImagesForVariantAsync(Guid variantId);
}
