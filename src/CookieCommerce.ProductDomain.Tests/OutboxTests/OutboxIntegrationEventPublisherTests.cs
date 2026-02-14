using CookieCommerce.ProductDomain.Infrastructure.Outbox;
using FluentAssertions;
using Marten;
using NSubstitute;
using System.Text.Json;

namespace CookieCommerce.ProductDomain.Tests.OutboxTests;

public class OutboxIntegrationEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_ShouldStoreEventInOutbox_WhenEventIsProvided()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var publisher = new OutboxIntegrationEventPublisher(session);
        var testEvent = new TestIntegrationEvent(Guid.NewGuid(), "Test");

        // Act
        await publisher.PublishAsync(testEvent, CancellationToken.None);

        // Assert
        session.Received(1).Store(Arg.Is<OutboxMessage>(m =>
            m.EventType == typeof(TestIntegrationEvent).AssemblyQualifiedName &&
            m.IsProcessed == false &&
            m.Payload.Contains(testEvent.Id.ToString())));
    }

    [Fact]
    public async Task PublishAsync_ShouldThrowArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var publisher = new OutboxIntegrationEventPublisher(session);

        // Act
        Func<Task> act = async () => await publisher.PublishAsync<TestIntegrationEvent>(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_ShouldSerializeEventCorrectly_WhenEventHasProperties()
    {
        // Arrange
        var session = Substitute.For<IDocumentSession>();
        var publisher = new OutboxIntegrationEventPublisher(session);
        var testEvent = new TestIntegrationEvent(Guid.NewGuid(), "Test Data");

        // Act
        await publisher.PublishAsync(testEvent, CancellationToken.None);

        // Assert
        session.Received(1).Store(Arg.Is<OutboxMessage>(m =>
            m.Payload.Contains(testEvent.Id.ToString()) &&
            m.Payload.Contains(testEvent.Name)));
    }

    private record TestIntegrationEvent(Guid Id, string Name);
}
