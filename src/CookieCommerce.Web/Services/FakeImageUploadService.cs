using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Services.Products;
using Microsoft.AspNetCore.Components.Forms;

namespace CookieCommerce.Web.Services;

public class FakeImageUploadService(IBackofficeService backofficeService) : IImageUploadService
{
    public async Task DeletePhotoAsync(Guid imageId, Guid variantId)
    {
        var variant = await backofficeService.GetVariantAsync(variantId);
        var imageToRemove = variant.Images.FirstOrDefault(i => i.Id == imageId);
        if (imageToRemove != null)
        {
            variant.Images.Remove(imageToRemove);
            if (imageToRemove.IsMain && variant.Images.Any())
            {
                variant.Images.First().IsMain = true;
            }
        }

    }

    public async Task<List<ProductVariantImageDto>> GetImagesForVariantAsync(Guid variantId)
    {
        var variant = await backofficeService.GetVariantAsync(variantId);
        return variant.Images.ToList();
    }

    public async Task<ProductVariantImageDto> ProcessImageUploadAsync(IBrowserFile file, Guid variantId)
    {
        long maxAllowedSize = 10 * 1024 * 1024;
        if (file.Size > maxAllowedSize) return null;

        var variant = await backofficeService.GetVariantAsync(variantId);

        using var ms = new MemoryStream();
        // This ensures the entire file is sucked into memory before we convert it
        await file.OpenReadStream(maxAllowedSize).CopyToAsync(ms);

        var buffer = ms.ToArray();
        var base64 = Convert.ToBase64String(buffer);
        var url = $"data:{file.ContentType};base64,{base64}";

        // Create the new Image DTO
        var newImage = new ProductVariantImageDto
        {
            Id = Guid.NewGuid(),
            Url = url,
            // If it's the first image, make it the main one automatically
            IsMain = !variant.Images.Any(i => i.IsMain)
        };

        variant.Images.Add(newImage);

        StaticInventoryList.InventoryItems
            .FirstOrDefault(i => i.Variants.Any(v => v.Id == variantId))?
            .Variants.FirstOrDefault(v => v.Id == variantId)?
            .Images.Add(newImage);

        if (newImage.IsMain)
        {
            // Get product based on variant id
            var productId = StaticInventoryList.InventoryItems
                .FirstOrDefault(i => i.Variants.Any(v => v.Id == variantId))?
                .ProductImageUrl = newImage.Url;
        }

        return newImage;
    }

    public async Task SetPhotoAsMain(Guid imageId, Guid variantId)
    {
        var variant = await backofficeService.GetVariantAsync(variantId);

        foreach (var img in variant.Images)
        {
            img.IsMain = false;
        }
    }
}