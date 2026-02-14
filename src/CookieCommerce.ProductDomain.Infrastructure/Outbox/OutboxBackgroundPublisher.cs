using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.ProductDomain.Infrastructure.Outbox;

/// <summary>
/// Background service that processes outbox messages and publishes them to the message bus.
/// This is currently stubbed and will be implemented in the future.
/// </summary>
public class OutboxBackgroundPublisher : BackgroundService
{
    private readonly ILogger<OutboxBackgroundPublisher> _logger;

    public OutboxBackgroundPublisher(ILogger<OutboxBackgroundPublisher> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxBackgroundPublisher is starting (stubbed).");

        // TODO: Implement outbox processing logic
        // 1. Query for unprocessed OutboxMessages from the database
        // 2. For each message, deserialize and publish to MassTransit
        // 3. Mark messages as processed
        // 4. Handle failures and retry logic
        // 5. Run in a loop with configurable interval (e.g., every 30 seconds)

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("OutboxBackgroundPublisher is running (no-op - stub implementation).");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("OutboxBackgroundPublisher is stopping.");
    }
}
