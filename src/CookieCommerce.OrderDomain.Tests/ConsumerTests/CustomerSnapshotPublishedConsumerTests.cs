using CookieCommerce.OrderDomain.Application.Customers.Integration;
using CookieCommerce.OrderDomain.Application.Customers.ReadModels;
using CookieCommerce.OrderDomain.Infrastructure.Consumers;
using FluentAssertions;
using Marten;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CookieCommerce.OrderDomain.Tests.ConsumerTests;

public class CustomerSnapshotPublishedConsumerTests
{
    private readonly IDocumentSession _session;
    private readonly ILogger<CustomerSnapshotPublishedConsumer> _logger;
    private readonly CustomerSnapshotPublishedConsumer _consumer;

    public CustomerSnapshotPublishedConsumerTests()
    {
        _session = Substitute.For<IDocumentSession>();
        _logger = Substitute.For<ILogger<CustomerSnapshotPublishedConsumer>>();
        _consumer = new CustomerSnapshotPublishedConsumer(_session, _logger);
    }

    [Fact]
    public async Task Consume_NewCustomer_CreatesSnapshot()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var @event = new CustomerSnapshotPublished
        {
            CustomerId = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var context = Substitute.For<ConsumeContext<CustomerSnapshotPublished>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        _session.LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns((CustomerSnapshotReadModel?)null);

        // Act
        await _consumer.Consume(context);

        // Assert
        _session.Received(1).Store(Arg.Is<CustomerSnapshotReadModel>(c =>
            c.Id == customerId &&
            c.FirstName == "John" &&
            c.LastName == "Doe" &&
            c.Email == "john.doe@example.com" &&
            c.DateOfBirth == new DateTime(1990, 1, 1)));

        await _session.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Consume_ExistingCustomer_UpdatesSnapshot()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var existingSnapshot = new CustomerSnapshotReadModel
        {
            Id = customerId,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            LastUpdated = DateTime.UtcNow.AddDays(-1)
        };

        var @event = new CustomerSnapshotPublished
        {
            CustomerId = customerId,
            FirstName = "Jane",
            LastName = "Doe",  // Name changed
            Email = "jane.doe@example.com",  // Email changed
            DateOfBirth = null
        };

        var context = Substitute.For<ConsumeContext<CustomerSnapshotPublished>>();
        context.Message.Returns(@event);
        context.CancellationToken.Returns(CancellationToken.None);

        _session.LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns(existingSnapshot);

        // Act
        await _consumer.Consume(context);

        // Assert
        existingSnapshot.FirstName.Should().Be("Jane");
        existingSnapshot.LastName.Should().Be("Doe");
        existingSnapshot.Email.Should().Be("jane.doe@example.com");
        existingSnapshot.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _session.Received(1).Store(existingSnapshot);
        await _session.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
