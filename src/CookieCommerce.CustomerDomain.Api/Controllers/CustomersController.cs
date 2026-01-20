using CookieCommerce.CustomerDomain.Api.Models.Customers;
using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CookieCommerce.CustomerDomain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost] // POST api/customers
    public async Task<IActionResult> CreateAsync([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var cmd = request.ToCommand();
        var customerId = await _mediator.Send(cmd, ct);
        return CreatedAtAction("GetById", new { id = customerId }, customerId);
    }

    [HttpGet("{id:guid}")] // GET api/customers/{id}
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var query = new GetCustomerById(id);
        var result = await _mediator.Send(query, ct);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id:guid}")] // DELETE api/customers/{id}
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ForgetCustomer(id), ct);
        return NoContent();
    }
}
