using CookieCommerce.ProductDomain.Application.Products.Media;
using CookieCommerce.ProductDomain.Infrastructure.Media;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CookieCommerce.ProductDomain.Tests.ApplicationTests;

public class ProductImageValidator_Tests
{
    [Fact]
    public async Task ValidateAsync_ReturnsFailure_GivenUnsupportedContentType()
    {
        var validator = CreateValidator();
        await using var stream = new MemoryStream(PngHeader());

        var result = await validator.ValidateAsync(stream, "text/plain", stream.Length, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("The uploaded image content type is not supported.");
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_GivenOversizedImage()
    {
        var validator = CreateValidator(maxBytes: 4);
        await using var stream = new MemoryStream(PngHeader());

        var result = await validator.ValidateAsync(stream, "image/png", stream.Length, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeds the maximum size");
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_GivenUnreadableDeclaredImage()
    {
        var validator = CreateValidator();
        await using var stream = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await validator.ValidateAsync(stream, "image/png", stream.Length, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("The uploaded file is not a readable image of the declared type.");
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuccess_GivenReadablePng()
    {
        var validator = CreateValidator();
        await using var stream = new MemoryStream(PngHeader());

        var result = await validator.ValidateAsync(stream, "image/png", stream.Length, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }

    private static ProductImageValidator CreateValidator(long maxBytes = 1024) => new(
        Options.Create(new ProductMediaOptions { MaxFileSizeBytes = maxBytes }));

    private static byte[] PngHeader() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
}
