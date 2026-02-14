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
        return Ok(result);
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

        return Ok(result);
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
        return CreatedAtAction(nameof(GetByIdAsync), new { id = productId }, productId);
    }

    /// <summary>
    /// Update an existing product (rename and/or change price)
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
