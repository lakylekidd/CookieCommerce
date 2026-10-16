using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;

namespace CookieCommerce.AuthenticationManagement.Api.Controllers;

[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = "Admin")]
public sealed class AccountAdministrationController(IKeycloakAdminService identities, IDocumentSession session,
    IHttpClientFactory clients) : ControllerBase
{
    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<KeycloakAccountStatus>> Status(Guid customerId, CancellationToken ct)
    {
        var identity = await identities.GetUserByCustomerIdAsync(customerId, ct);
        if (identity is null) return new KeycloakAccountStatus(null, "Missing login identity");
        var status = await identities.GetAccountStatusAsync(identity.Id, ct);
        var identityId = identity.Id;
        var recovery = await session.Query<PasswordRecoveryDelivery>().Where(x => x.IdentityId == identityId).OrderByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        return status with { LastRecoveryState = recovery?.State };
    }

    [HttpPost("{customerId:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid customerId, CancellationToken ct)
    {
        var identity = await identities.GetUserByCustomerIdAsync(customerId, ct);
        if (identity is null) return NotFound();
        await RunAudited(customerId, "Unblock account", () => identities.ClearLoginFailuresAsync(identity.Id, ct), ct);
        return NoContent();
    }

    [HttpPost("{customerId:guid}/password-reset")]
    public async Task<IActionResult> Reset(Guid customerId, CancellationToken ct)
    {
        var identity = await identities.GetUserByCustomerIdAsync(customerId, ct);
        if (identity is null) return NotFound();
        await RunAudited(customerId, "Request password reset email", async () =>
        {
            session.Store(new PasswordRecoveryDelivery { IdentityId = identity.Id }); await session.SaveChangesAsync(ct);
        }, ct);
        return Accepted();
    }

    [HttpPost("{customerId:guid}/invite")]
    public async Task<IActionResult> Invite(Guid customerId, CancellationToken ct)
    {
        var customer = await clients.CreateClient("customer-domain-api").GetFromJsonAsync<CustomerIdentity>("api/customer/" + customerId, ct);
        if (customer is null) return NotFound();
        await RunAudited(customerId, "Send password setup invitation", async () =>
        {
            var id = await identities.ProvisionCustomerAsync(customerId, customer.Email, customer.FirstName, customer.LastName, ct);
            var authRecord = await session.Query<AuthUserReadModel>().FirstOrDefaultAsync(x => x.KeycloakId == id, ct);
            if (authRecord is null)
            {
                var aggregate = new AuthUserAggregate();
                aggregate.Register(Guid.CreateVersion7(), id, customer.Email, customer.FirstName, customer.LastName, customerId);
                session.Events.StartStream<AuthUserAggregate>(aggregate.Id, aggregate.DomainEvents);
                await session.SaveChangesAsync(ct);
            }
            session.Store(new PasswordRecoveryDelivery { IdentityId = id }); await session.SaveChangesAsync(ct);
        }, ct);
        return Accepted();
    }

    [HttpPut("email-settings")]
    public async Task<IActionResult> ConfigureEmail(KeycloakEmailSettings settings, CancellationToken ct)
    {
        if (!Uri.TryCreate(settings.PublicStoreUrl, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https")
            || settings.Port is < 1 or > 65535 || settings.Security is not ("None" or "Ssl" or "StartTls")) return BadRequest();
        await RunAudited(Guid.Empty, "Configure outbound authentication email", () => identities.ConfigureEmailAsync(settings, ct), ct);
        return NoContent();
    }

    [HttpGet("recovery-deliveries")]
    public async Task<IActionResult> RecoveryDeliveries(CancellationToken ct) =>
        Ok(await session.Query<PasswordRecoveryDelivery>().OrderByDescending(x => x.Id).Take(20).ToListAsync(ct));

    private async Task RunAudited(Guid customerId, string action, Func<Task> run, CancellationToken ct)
    {
        var audit = new AccountAdministrationAudit
        {
            Id = Guid.CreateVersion7(), CustomerId = customerId, ActorId = User.FindFirst("sub")!.Value,
            Action = action, At = DateTimeOffset.UtcNow, Outcome = "Started"
        };
        session.Store(audit);
        await session.SaveChangesAsync(ct);
        try { await run(); audit.Outcome = "Succeeded"; }
        catch { audit.Outcome = "Failed"; throw; }
        finally { session.Store(audit); await session.SaveChangesAsync(CancellationToken.None); }
    }

    private sealed record CustomerIdentity(string Email, string FirstName, string LastName);
}

public sealed class AccountAdministrationAudit
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string ActorId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Outcome { get; set; } = "";
    public DateTimeOffset At { get; set; }
}
