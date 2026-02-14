using Marten;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CookieCommerce.ProductDomain.Infrastructure.Outbox;

/// <summary>
/// Background service that processes outbox messages and publishes them to the message bus.
/// Polls the outbox table at regular intervals and publishes unprocessed messages.
/// </summary>
public class OutboxBackgroundPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxBackgroundPublisher> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 100;

    public OutboxBackgroundPublisher(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxBackgroundPublisher> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxBackgroundPublisher is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxBackgroundPublisher is stopping.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var documentStore = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await using var session = documentStore.LightweightSession();

        // Query for unprocessed messages ordered by creation date
        var unprocessedMessages = await session
            .Query<OutboxMessage>()
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (unprocessedMessages.Count == 0)
        {
            _logger.LogDebug("No unprocessed outbox messages found.");
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages.", unprocessedMessages.Count);

        foreach (var message in unprocessedMessages)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await PublishMessageAsync(message, publishEndpoint, cancellationToken);

                // Mark as processed
                message.IsProcessed = true;
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
                session.Update(message);

                _logger.LogDebug("Successfully published outbox message {MessageId} of type {EventType}.",
                    message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId} of type {EventType}.",
                    message.Id, message.EventType);

                // Store error information but don't mark as processed (will retry)
                message.Error = ex.Message;
                session.Update(message);
            }
        }

        await session.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Completed processing {Count} outbox messages.", unprocessedMessages.Count);
    }

    private async Task PublishMessageAsync(OutboxMessage message, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken)
    {
        // Deserialize the event type
        var eventType = Type.GetType(message.EventType);
        if (eventType == null)
        {
            throw new InvalidOperationException($"Could not resolve event type: {message.EventType}");
        }

        // Deserialize the payload
        var @event = JsonSerializer.Deserialize(message.Payload, eventType);
        if (@event == null)
        {
            throw new InvalidOperationException($"Failed to deserialize event payload for message {message.Id}");
        }

        // Publish to MassTransit
        await publishEndpoint.Publish(@event, eventType, cancellationToken);
    }
}
