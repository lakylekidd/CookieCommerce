using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CookieCommerce.AuthenticationManagement.Infrastructure;

internal sealed class PasswordRecoveryDeliveryWorker(IServiceScopeFactory scopes, ILogger<PasswordRecoveryDeliveryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
                var users = scope.ServiceProvider.GetRequiredService<IKeycloakAdminService>();
                var now = DateTimeOffset.UtcNow;
                var pending = await session.Query<PasswordRecoveryDelivery>()
                    .Where(x => (x.State == "Queued" || x.State == "Retrying") && x.RetryAfter <= now).Take(20).ToListAsync(ct);
                foreach (var item in pending)
                {
                    await using var guard = new NpgsqlConnection(scope.ServiceProvider.GetRequiredService<IConfiguration>().GetConnectionString("AuthenticationManagement"));
                    await guard.OpenAsync(ct); await using var transaction = await guard.BeginTransactionAsync(ct);
                    await using var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended($1, 0))", guard, transaction);
                    command.Parameters.AddWithValue("recovery-delivery:" + item.Id); await command.ExecuteNonQueryAsync(ct);
                    var delivery = await session.LoadAsync<PasswordRecoveryDelivery>(item.Id, ct);
                    if (delivery is null || delivery.State is not ("Queued" or "Retrying")) continue;
                    if (delivery.ExpiresAt <= DateTimeOffset.UtcNow) delivery.State = "Expired";
                    else
                    {
                        try { await users.SendPasswordResetAsync(delivery.IdentityId, ct); delivery.State = "Sent"; delivery.SentAt = DateTimeOffset.UtcNow; }
                        catch (Exception ex) when (!ct.IsCancellationRequested)
                        {
                            delivery.Attempts++; delivery.State = delivery.Attempts >= 5 ? "Failed" : "Retrying";
                            delivery.RetryAfter = DateTimeOffset.UtcNow.AddMinutes(Math.Pow(2, delivery.Attempts));
                            logger.LogWarning("Password recovery delivery {DeliveryId} failed ({FailureType})", delivery.Id, ex.GetType().Name);
                        }
                    }
                    session.Store(delivery); await session.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested) { logger.LogWarning("Password recovery queue will retry ({FailureType})", ex.GetType().Name); }
        }
    }
}
