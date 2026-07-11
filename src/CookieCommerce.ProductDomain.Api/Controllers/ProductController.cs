using CookieCommerce.ProductDomain.Api.Models.Products;
using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CookieCommerce.ProductDomain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct = default)
    {
        var query = new GetAllProducts();
        var result = await _mediator.Send(query, ct);
        return Ok(result.Select(product => product.ToResponse()));
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var query = new GetProductById(id);
        var result = await _mediator.Send(query, ct);

        if (result == null) return NotFound();

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var cmd = request.ToCommand();
        var productId = await _mediator.Send(cmd, ct);
        return CreatedAtAction("GetById", new { id = productId }, productId);
    }

    /// <summary>
    /// Update an existing product (rename and/or change price)
    /// Note: This performs two separate commands sequentially. In high-concurrency scenarios,
    /// consider creating a single UpdateProduct command for atomic updates.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // First, rename the product
        var renameCmd = request.ToRenameCommand(id);
        await _mediator.Send(renameCmd, ct);

        // Then, change the price
        var changePriceCmd = request.ToChangePriceCommand(id);
        await _mediator.Send(changePriceCmd, ct);

        if (request.Variants.Count > 0)
            await _mediator.Send(request.ToVariantCommand(id), ct);

        return NoContent();
    }

    [HttpPut("{id:guid}/variants")]
    public async Task<IActionResult> UpsertVariantsAsync(Guid id, [FromBody] List<ProductVariantRequest> request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        await _mediator.Send(request.ToVariantCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadVariantPhotoAsync(
        Guid productId,
        Guid variantId,
        IFormFile? file,
        [FromForm] string? altText,
        [FromForm] int? sortOrder,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("file", "An image file is required.");
            return ValidationProblem(ModelState);
        }

        await using var stream = file.OpenReadStream();
        var mediaAsset = await _mediator.Send(new UploadVariantPhotoCommand(
            productId,
            variantId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            altText,
            sortOrder), ct);

        return Created($"/api/product/{productId}/variants/{variantId}/photos/{mediaAsset.Id}", mediaAsset.ToResponse());
    }

    [HttpDelete("{productId:guid}/variants/{variantId:guid}/photos/{mediaAssetId:guid}")]
    public async Task<IActionResult> RemoveVariantPhotoAsync(Guid productId, Guid variantId, Guid mediaAssetId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveVariantPhotoCommand(productId, variantId, mediaAssetId), ct);
        return NoContent();
    }

    [HttpPost("{productId:guid}/media/main-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMainImageAsync(
        Guid productId,
        IFormFile? file,
        [FromForm] string? altText,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("file", "An image file is required.");
            return ValidationProblem(ModelState);
        }

        await using var stream = file.OpenReadStream();
        var mediaAsset = await _mediator.Send(new UploadProductMainImageCommand(
            productId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            altText), ct);

        return Created($"/api/product/{productId}/media/main-image/{mediaAsset.Id}", mediaAsset.ToResponse());
    }

    [HttpDelete("{productId:guid}/media/main-image/{mediaAssetId:guid}")]
    public async Task<IActionResult> RemoveMainImageAsync(Guid productId, Guid mediaAssetId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveProductMainImageCommand(productId, mediaAssetId), ct);
        return NoContent();
    }

    /// <summary>
    /// Delete a product (soft delete - deactivates the product)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateProduct(id), ct);
        return NoContent();
    }
}
