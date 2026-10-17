using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using System.Security.Cryptography;

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
        var roles = await identities.GetRealmRolesAsync(identityId, ct);
        return status with
        {
            LastRecoveryState = recovery?.State,
            IsAdmin = roles.Any(role => role.Equals("Admin", StringComparison.OrdinalIgnoreCase)),
            IsCurrentUser = identityId.Equals(User.FindFirst("sub")?.Value, StringComparison.Ordinal)
        };
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
    public async Task<ActionResult<PasswordSetupInvitationResponse>> Invite(Guid customerId, CancellationToken ct)
    {
        var customer = await GetCustomerAsync(customerId, ct);
        if (customer is null) return NotFound();
        PasswordSetupInvitationResponse? invitation = null;
        await RunAudited(customerId, "Send password setup invitation", async () =>
        {
            var id = await EnsureAccountAsync(customerId, customer, ct);
            invitation = await IssueInvitationAsync(id, customer, ct);
        }, ct);
        return Accepted(invitation);
    }

    [HttpPost("{customerId:guid}/provision")]
    public async Task<IActionResult> Provision(Guid customerId, CancellationToken ct)
    {
        var customer = await GetCustomerAsync(customerId, ct);
        if (customer is null) return NotFound();
        await RunAudited(customerId, "Provision customer account", () => EnsureAccountAsync(customerId, customer, ct), ct);
        return NoContent();
    }

    [HttpPut("{customerId:guid}/role/admin")]
    public async Task<IActionResult> UpgradeToAdmin(Guid customerId, CancellationToken ct)
    {
        var identity = await identities.GetUserByCustomerIdAsync(customerId, ct);
        if (identity is null) return NotFound();
        await RunAudited(customerId, "Upgrade customer to administrator", () => identities.AssignRealmRoleAsync(identity.Id, "Admin", ct), ct);
        return NoContent();
    }

    [HttpDelete("{customerId:guid}/role/admin")]
    public async Task<IActionResult> DowngradeToCustomer(Guid customerId, CancellationToken ct)
    {
        var identity = await identities.GetUserByCustomerIdAsync(customerId, ct);
        if (identity is null) return NotFound();
        if (identity.Id.Equals(User.FindFirst("sub")?.Value, StringComparison.Ordinal))
            return Conflict(new ProblemDetails { Title = "You cannot downgrade your own account." });

        var roles = await identities.GetRealmRolesAsync(identity.Id, ct);
        if (!roles.Any(role => role.Equals("Admin", StringComparison.OrdinalIgnoreCase))) return NoContent();
        if (await identities.CountUsersInRealmRoleAsync("Admin", ct) <= 1)
            return Conflict(new ProblemDetails { Title = "At least one administrator account is required." });

        await using var guard = new Npgsql.NpgsqlConnection(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("AuthenticationManagement"));
        await guard.OpenAsync(ct);
        await using var transaction = await guard.BeginTransactionAsync(ct);
        await using var command = new Npgsql.NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended($1, 0))", guard, transaction);
        command.Parameters.AddWithValue("administrator-role-mutation");
        await command.ExecuteNonQueryAsync(ct);
        if (await identities.CountUsersInRealmRoleAsync("Admin", ct) <= 1)
            return Conflict(new ProblemDetails { Title = "At least one administrator account is required." });
        await RunAudited(customerId, "Downgrade administrator to customer", async () =>
        {
            await identities.AssignRealmRoleAsync(identity.Id, "User", ct);
            await identities.RemoveRealmRoleAsync(identity.Id, "Admin", ct);
        }, ct);
        await transaction.CommitAsync(ct);
        return NoContent();
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

    private async Task<CustomerIdentity?> GetCustomerAsync(Guid customerId, CancellationToken ct) =>
        await clients.CreateClient("customer-domain-api").GetFromJsonAsync<CustomerIdentity>("api/customer/" + customerId, ct);

    private async Task<string> EnsureAccountAsync(Guid customerId, CustomerIdentity customer, CancellationToken ct)
    {
        var id = await identities.ProvisionCustomerAsync(customerId, customer.Email, customer.FirstName, customer.LastName, ct);
        var authRecord = await session.Query<AuthUserReadModel>().FirstOrDefaultAsync(x => x.KeycloakId == id, ct);
        if (authRecord is not null) return id;
        var aggregate = new AuthUserAggregate();
        aggregate.Register(Guid.CreateVersion7(), id, customer.Email, customer.FirstName, customer.LastName, customerId);
        session.Events.StartStream<AuthUserAggregate>(aggregate.Id, aggregate.DomainEvents);
        await session.SaveChangesAsync(ct);
        return id;
    }

    private async Task<PasswordSetupInvitationResponse> IssueInvitationAsync(string identityId, CustomerIdentity customer, CancellationToken ct)
    {
        var existing = await session.Query<PasswordSetupInvitation>()
            .Where(x => x.IdentityId == identityId && x.UsedAt == null && x.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var item in existing) item.RevokedAt = DateTimeOffset.UtcNow;

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        session.Store(existing.ToArray());
        session.Store(new PasswordSetupInvitation
        {
            Id = PasswordSetupInvitation.HashToken(token), IdentityId = identityId, Email = customer.Email, ExpiresAt = expiresAt
        });
        await session.SaveChangesAsync(ct);
        return new PasswordSetupInvitationResponse(token, customer.Email, customer.FirstName, customer.LastName, expiresAt);
    }

    private sealed record CustomerIdentity(string Email, string FirstName, string LastName);
}

public sealed record PasswordSetupInvitationResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset ExpiresAt);

public sealed class AccountAdministrationAudit
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string ActorId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Outcome { get; set; } = "";
    public DateTimeOffset At { get; set; }
}
