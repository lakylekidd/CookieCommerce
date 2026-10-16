using CookieCommerce.AuthenticationManagement.Api.Models;
using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using Microsoft.AspNetCore.RateLimiting;
using Marten;
using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using Microsoft.AspNetCore.Authorization;

namespace CookieCommerce.AuthenticationManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender mediator, IKeycloakTokenService tokens, IKeycloakAdminService users) : ControllerBase
{
    public sealed record RefreshRequest(string RefreshToken);
    public sealed record ForgotPasswordRequest(string Email);

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return BadRequest();
        return Ok(await tokens.RefreshAsync(request.RefreshToken, ct));
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("recovery")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email.Trim())) return BadRequest();
        var email = request.Email.Trim().ToLowerInvariant();
        var session = HttpContext.RequestServices.GetRequiredService<IDocumentSession>();
        var key = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(email)));
        await using var guard = new Npgsql.NpgsqlConnection(HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("AuthenticationManagement"));
        await guard.OpenAsync(ct);
        await using var transaction = await guard.BeginTransactionAsync(ct);
        await using var command = new Npgsql.NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended($1, 0))", guard, transaction);
        command.Parameters.AddWithValue("recovery:" + key);
        await command.ExecuteNonQueryAsync(ct);
        var cooldown = await session.LoadAsync<RecoveryCooldown>(key, ct);
        if (cooldown?.RetryAfter > DateTimeOffset.UtcNow) return Accepted();
        session.Store(new RecoveryCooldown { Id = key, RetryAfter = DateTimeOffset.UtcNow.AddMinutes(2) });
        await session.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        try
        {
            var user = await users.GetUserByEmailAsync(email, ct);
            if (user is not null)
            {
                session.Store(new PasswordRecoveryDelivery { IdentityId = user.Id });
                await session.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>()
                .LogWarning("Password recovery delivery failed ({FailureType})", ex.GetType().Name);
        }
        return Accepted();
    }
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
    [Authorize]
    public async Task<IActionResult> GetProfileAsync(Guid userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfile(userId), ct);
        if (!User.IsInRole("Admin") && result.KeycloakId != User.FindFirst("sub")?.Value) return Forbid();
        return Ok(result);
    }
}

public sealed class RecoveryCooldown
{
    public string Id { get; set; } = "";
    public DateTimeOffset RetryAfter { get; set; }
}
