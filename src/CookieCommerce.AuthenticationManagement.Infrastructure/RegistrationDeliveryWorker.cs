using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using CookieCommerce.DomainBuildingBlocks;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.AuthenticationManagement.Infrastructure;

internal sealed class RegistrationDeliveryWorker(IServiceScopeFactory scopes, ILogger<RegistrationDeliveryWorker> logger) : BackgroundService
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
                var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
                var pending = await session.Query<RegistrationDelivery>().Where(x => x.PublishedAt == null).Take(50).ToListAsync(ct);
                foreach (var delivery in pending)
                {
                    await publisher.PublishAsync(delivery.Message, ct);
                    delivery.PublishedAt = DateTimeOffset.UtcNow;
                    session.Store(delivery);
                    await session.SaveChangesAsync(ct);
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested) { logger.LogWarning("Registration delivery will retry ({FailureType})", ex.GetType().Name); }
        }
    }
}
