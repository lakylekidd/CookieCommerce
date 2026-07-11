using CookieCommerce.ProductDomain.Application.Products.Media;
using Microsoft.Extensions.Options;

namespace CookieCommerce.ProductDomain.Infrastructure.Media;

public sealed class ProductImageValidator(IOptions<ProductMediaOptions> options) : IProductImageValidator
{
    private readonly ProductMediaOptions _options = options.Value;

    public async Task<ProductImageValidationResult> ValidateAsync(
        Stream imageStream,
        string contentType,
        long lengthBytes,
        CancellationToken cancellationToken)
    {
        if (imageStream is null)
            return ProductImageValidationResult.Failure("An image file is required.");

        if (lengthBytes <= 0)
            return ProductImageValidationResult.Failure("The uploaded image is empty.");

        if (lengthBytes > _options.MaxFileSizeBytes)
            return ProductImageValidationResult.Failure($"The uploaded image exceeds the maximum size of {_options.MaxFileSizeBytes} bytes.");

        if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ProductImageValidationResult.Failure("The uploaded image content type is not supported.");

        if (!imageStream.CanSeek)
            return ProductImageValidationResult.Failure("The uploaded image stream must be seekable for validation.");

        var originalPosition = imageStream.Position;
        try
        {
            var header = new byte[12];
            var bytesRead = await imageStream.ReadAsync(header, cancellationToken);

            if (!MatchesKnownImageHeader(header.AsSpan(0, bytesRead), contentType))
                return ProductImageValidationResult.Failure("The uploaded file is not a readable image of the declared type.");

            return ProductImageValidationResult.Success;
        }
        finally
        {
            imageStream.Position = originalPosition;
        }
    }

    private static bool MatchesKnownImageHeader(ReadOnlySpan<byte> header, string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => header.Length >= 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            "image/webp" => header.Length >= 12
                && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
            _ => false
        };
    }
}
