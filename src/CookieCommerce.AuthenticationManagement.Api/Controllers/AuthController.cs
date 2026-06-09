using CookieCommerce.AuthenticationManagement.Api.Models;
using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CookieCommerce.AuthenticationManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = await _mediator.Send(new RegisterUser(request.Email, request.Password, request.FirstName, request.LastName), ct);
        return Created("", userId);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _mediator.Send(new LoginUser(request.Email, request.Password), ct);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _mediator.Send(new LogoutUser(request.RefreshToken), ct);
        return NoContent();
    }

    [HttpGet("profile/{userId:guid}")]
    public async Task<IActionResult> GetProfileAsync(Guid userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfile(userId), ct);
        return Ok(result);
    }
}
