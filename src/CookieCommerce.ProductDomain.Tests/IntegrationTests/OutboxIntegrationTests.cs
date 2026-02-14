using CookieCommerce.ProductDomain.Application.Products.Commands;
using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Infrastructure.Outbox;
using FluentAssertions;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.ProductDomain.Tests.IntegrationTests;

public class OutboxIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private IServiceScope? _scope;
    private IDocumentStore? _store;

    public OutboxIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Factory is already initialized by IClassFixture, just reset the database
        await _factory.ResetDatabaseAsync();
        _scope = _factory.Services.CreateScope();
        _store = _scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        // Don't dispose factory here - IClassFixture handles that
    }

    [Fact]
    public async Task CreateProduct_ShouldStoreEventAndOutboxMessage_Atomically()
    {
        // Arrange
        var mediator = _scope!.ServiceProvider.GetRequiredService<ISender>();
        var command = new CreateProduct("Test Product", "Test Description", 99.99m);

        // Act
        var productId = await mediator.Send(command, CancellationToken.None);

        // Assert - Verify product event was stored
        await using var session = _store!.QuerySession();
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(productId);
        product.Should().NotBeNull();
        product!.Name.Should().Be("Test Product");

        // Assert - Verify outbox message was stored atomically with the domain event
        var outboxMessages = await session.Query<OutboxMessage>().ToListAsync();
        outboxMessages.Should().ContainSingle(m => m.EventType!.Contains("ProductSnapshotPublished"));
    }

    [Fact]
    public async Task DeactivateProduct_ShouldStoreOutboxMessage_Atomically()
    {
        // Arrange
        var mediator = _scope!.ServiceProvider.GetRequiredService<ISender>();
        
        // First create a product
        var createCommand = new CreateProduct("Test Product", "Test Description", 99.99m);
        var productId = await mediator.Send(createCommand, CancellationToken.None);

        // Act - Deactivate the product
        var deactivateCommand = new DeactivateProduct(productId);
        await mediator.Send(deactivateCommand, CancellationToken.None);

        // Assert - Both the deactivation event and outbox message should be committed together
        await using var session = _store!.QuerySession();
        var product = await session.Events.AggregateStreamAsync<ProductAggregate>(productId);
        product.Should().NotBeNull();
        product!.IsActive.Should().BeFalse();

        // Verify integration event was stored in outbox atomically
        var outboxMessages = await session.Query<OutboxMessage>().ToListAsync();
        outboxMessages.Should().Contain(m => m.EventType!.Contains("ProductDeactivatedIntegrationEvent"));
    }

    [Fact]
    public async Task OutboxMessage_ShouldBeIndexedByIsProcessed()
    {
        // Arrange
        await using var session = _store!.LightweightSession();
        
        var message1 = new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EventType = "TestEvent",
            Payload = "{}",
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        var message2 = new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EventType = "TestEvent",
            Payload = "{}",
            CreatedAt = DateTime.UtcNow,
            IsProcessed = true,
            ProcessedAt = DateTime.UtcNow
        };

        // Act
        session.Store(message1, message2);
        await session.SaveChangesAsync();

        // Assert
        await using var querySession = _store.QuerySession();
        var unprocessed = await querySession.Query<OutboxMessage>()
            .Where(m => !m.IsProcessed)
            .ToListAsync();

        unprocessed.Should().HaveCount(1);
        unprocessed.First().Id.Should().Be(message1.Id);
    }
}
