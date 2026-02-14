using Marten;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;
using System.Threading.Channels;

namespace CookieCommerce.ProductDomain.Infrastructure.Outbox;

/// <summary>
/// Background service that processes outbox messages using a hybrid approach:
/// - PostgreSQL LISTEN/NOTIFY for immediate processing
/// - Fallback polling every 30 seconds as a safety net
/// 
/// The service maintains a dedicated connection to listen for PostgreSQL notifications
/// sent by the outbox_notify trigger whenever a new OutboxMessage is inserted.
/// This ensures near-instant message delivery while still polling periodically
/// to catch any messages that might have been missed.
/// </summary>
public class OutboxBackgroundPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OutboxBackgroundPublisher> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 100;
    private const string NotificationChannel = "outbox_channel";
    
    private readonly Channel<bool> _notificationChannel;
    private NpgsqlConnection? _listenerConnection;
    private Task? _listenerTask;

    public OutboxBackgroundPublisher(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<OutboxBackgroundPublisher> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
        _notificationChannel = Channel.CreateUnbounded<bool>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxBackgroundPublisher is starting with hybrid LISTEN/NOTIFY + polling.");

        try
        {
            // Start the PostgreSQL LISTEN task
            _listenerTask = Task.Run(() => ListenForNotificationsAsync(stoppingToken), stoppingToken);

            // Main processing loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for either a notification or timeout
                    var notificationReceived = await WaitForNotificationOrTimeoutAsync(stoppingToken);
                    
                    if (notificationReceived)
                    {
                        _logger.LogDebug("Processing outbox messages triggered by NOTIFY event.");
                    }
                    else
                    {
                        _logger.LogDebug("Processing outbox messages triggered by polling timeout.");
                    }

                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error occurred while processing outbox messages.");
                }
            }
        }
        finally
        {
            await CleanupListenerAsync();
        }

        _logger.LogInformation("OutboxBackgroundPublisher is stopping.");
    }

    /// <summary>
    /// Waits for either a NOTIFY signal or timeout equal to the polling interval.
    /// Returns true if notification was received, false if timeout occurred.
    /// </summary>
    private async Task<bool> WaitForNotificationOrTimeoutAsync(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_pollingInterval);

        try
        {
            // Wait for a notification from the channel or until timeout/cancellation
            await _notificationChannel.Reader.ReadAsync(cts.Token);
            return true; // Notification received
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false; // Timeout occurred (polling interval elapsed)
        }
        catch (OperationCanceledException)
        {
            throw; // Actual cancellation, propagate up
        }
    }

    /// <summary>
    /// Maintains a dedicated PostgreSQL connection and listens for NOTIFY events.
    /// Runs in the background and signals the processing loop via a channel.
    /// </summary>
    private async Task ListenForNotificationsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("ProductDomain");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Connection string 'ProductDomain' not found for LISTEN/NOTIFY.");
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    continue;
                }

                // Create a dedicated long-lived connection for LISTEN
                _listenerConnection = new NpgsqlConnection(connectionString);
                await _listenerConnection.OpenAsync(cancellationToken);

                // Set up notification handler
                _listenerConnection.Notification += (sender, args) =>
                {
                    _logger.LogDebug("Received PostgreSQL NOTIFY on channel '{Channel}' with payload: {Payload}", 
                        args.Channel, args.Payload);
                    
                    // Signal the processing loop
                    _notificationChannel.Writer.TryWrite(true);
                };

                // Start listening on the outbox channel
                using var cmd = new NpgsqlCommand($"LISTEN {NotificationChannel}", _listenerConnection);
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                _logger.LogInformation("Started listening on PostgreSQL channel '{Channel}'.", NotificationChannel);

                // Wait for notifications indefinitely
                while (!cancellationToken.IsCancellationRequested)
                {
                    await _listenerConnection.WaitAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LISTEN/NOTIFY connection. Will retry in 30 seconds.");
                
                // Clean up the connection
                if (_listenerConnection != null)
                {
                    try
                    {
                        await _listenerConnection.CloseAsync();
                        await _listenerConnection.DisposeAsync();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                    _listenerConnection = null;
                }

                // Wait before retry
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Cleans up the listener connection on shutdown.
    /// </summary>
    private async Task CleanupListenerAsync()
    {
        _logger.LogInformation("Cleaning up LISTEN/NOTIFY connection.");

        if (_listenerConnection != null)
        {
            try
            {
                // Unlisten from the channel
                if (_listenerConnection.State == System.Data.ConnectionState.Open)
                {
                    using var cmd = new NpgsqlCommand($"UNLISTEN {NotificationChannel}", _listenerConnection);
                    await cmd.ExecuteNonQueryAsync();
                }

                await _listenerConnection.CloseAsync();
                await _listenerConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning up LISTEN/NOTIFY connection.");
            }
            finally
            {
                _listenerConnection = null;
            }
        }

        if (_listenerTask != null)
        {
            try
            {
                await _listenerTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error waiting for listener task to complete.");
            }
        }
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
