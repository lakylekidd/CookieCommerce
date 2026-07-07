using CookieCommerce.CustomerDomain.Api.Models.Customers;
using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.Queries;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CookieCommerce.CustomerDomain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    // ─── /me Profile Endpoints (US1) ──────────────────────────────────────────

    /// <summary>
    /// Returns the authenticated customer's own profile.
    /// The customer id is resolved from the X-Customer-Id header (US3 will enforce JWT auth).
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfileAsync(CancellationToken ct = default)
    {
        var customerId = ResolveCustomerId();
        if (customerId is null) return Unauthorized();

        var query = new GetCurrentCustomerProfileQuery(customerId.Value);
        var result = await _mediator.Send(query, ct);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Updates the authenticated customer's own editable profile details.
    /// Non-email fields save immediately; email changes become pending verification.
    /// </summary>
    [HttpPatch("me/profile")]
    public async Task<IActionResult> UpdateMyProfileAsync(
        [FromBody] UpdateCustomerProfileRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var customerId = ResolveCustomerId();
        if (customerId is null) return Unauthorized();

        var correlationId = HttpContext.TraceIdentifier;
        var command = request.ToCommand(customerId.Value, correlationId);
        var result = await _mediator.Send(command, ct);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Requests or resends email verification for the authenticated customer's pending email.
    /// </summary>
    [HttpPost("me/email-verification")]
    public async Task<IActionResult> ResendEmailVerificationAsync(
        [FromBody] ResendCustomerEmailVerificationRequest? request,
        CancellationToken ct = default)
    {
        var customerId = ResolveCustomerId();
        if (customerId is null) return Unauthorized();

        var correlationId = HttpContext.TraceIdentifier;
        var command = (request ?? new ResendCustomerEmailVerificationRequest(null))
            .ToCommand(customerId.Value, correlationId);

        var result = await _mediator.Send(command, ct);
        return Accepted(new ResendCustomerEmailVerificationResponse(result.Status.ToString(), result.Message));
    }

    /// <summary>
    /// Confirms the authenticated customer's pending email verification token.
    /// </summary>
    [HttpPost("me/email-verification/confirm")]
    public async Task<IActionResult> ConfirmEmailVerificationAsync(
        [FromBody] ConfirmCustomerEmailVerificationRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var customerId = ResolveCustomerId();
        if (customerId is null) return Unauthorized();

        var command = request.ToCommand(customerId.Value, HttpContext.TraceIdentifier);
        var result = await _mediator.Send(command, ct);
        return Ok(result.ToResponse());
    }

    // ─── Existing Customer Endpoints ──────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var cmd = request.ToCommand();
        var customerId = await _mediator.Send(cmd, ct);
        return CreatedAtAction("GetById", new { id = customerId }, customerId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var query = new GetCustomerById(id);
        var result = await _mediator.Send(query, ct);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct = default)
    {
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ForgetCustomer(id), ct);
        return NoContent();
    }

    [HttpPost("{customerId:guid}/addresses/")]
    public async Task<IActionResult> AddAddressAsync(Guid customerId, [FromBody] AddCustomerAddressRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var cmd = request.ToCommand(customerId);
        await _mediator.Send(cmd, ct);
        return CreatedAtAction("GetById", new { id = customerId }, customerId);
    }

    [HttpDelete("{customerId:guid}/addresses/{label:alpha}")]
    public async Task<IActionResult> DeleteAddressAsync(Guid customerId, string label, CancellationToken ct)
    {
        await _mediator.Send(new RemoveCustomerAddress(customerId, label), ct);
        return NoContent();
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the current customer id from the authenticated context.
    /// US1: reads X-Customer-Id header (caller must be authenticated at the gateway/web layer).
    /// US3 (T071) will replace this with proper JWT claim enforcement.
    /// </summary>
    private Guid? ResolveCustomerId()
    {
        var header = Request.Headers["X-Customer-Id"].FirstOrDefault();
        if (Guid.TryParse(header, out var id)) return id;

        // Also try NameIdentifier claim for JWT-authenticated requests
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(claim, out id)) return id;

        return null;
    }
}
