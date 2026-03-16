using CookieCommerce.Web.Models.Products;
using Microsoft.AspNetCore.Components.Forms;

namespace CookieCommerce.Web.Services;

public interface IImageUploadService
{
    Task<ProductVariantImageDto> ProcessImageUploadAsync(IBrowserFile file, Guid variantId);
    Task SetPhotoAsMain(Guid imageId, Guid variantId);
    Task DeletePhotoAsync(Guid imageId, Guid variantId);
    Task<List<ProductVariantImageDto>> GetImagesForVariantAsync(Guid variantId);
}
