using CookieCommerce.OrderDomain.Application.Customers.Integration;
using CookieCommerce.OrderDomain.Application.Customers.ReadModels;
using CookieCommerce.OrderDomain.Infrastructure.Consumers;
using FluentAssertions;
using Marten;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CookieCommerce.OrderDomain.Tests.ConsumerTests;

public class CustomerDeletedConsumerTests
{
    private readonly IDocumentSession _session;
    private readonly ILogger<CustomerDeletedConsumer> _logger;
    private readonly CustomerDeletedConsumer _consumer;

    public CustomerDeletedConsumerTests()
    {
        _session = Substitute.For<IDocumentSession>();
        _logger = Substitute.For<ILogger<CustomerDeletedConsumer>>();
        _consumer = new CustomerDeletedConsumer(_session, _logger);
    }

    [Fact]
    public async Task Consume_CustomerDeleted_RemovesSnapshot()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var @event = new CustomerDeletedIntegrationEvent(customerId);

        var context = Substitute.For<ConsumeContext<CustomerDeletedIntegrationEvent>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert
        _session.Received(1).Delete<CustomerSnapshotReadModel>(customerId);
        await _session.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
