using CookieCommerce.OrderDomain.Application.Customers.ReadModels;
using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Orders.Handlers;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.OrderDomain.Tests.ApplicationTests;

public class PlaceOrderHandlerTests
{
    private readonly IDocumentSession _session;
    private readonly PlaceOrderHandler _handler;

    public PlaceOrderHandlerTests()
    {
        _session = Substitute.For<IDocumentSession>();
        _handler = new PlaceOrderHandler(_session);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewOrderId()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var shippingAddress = new OrderAddress("123 Main St", "Springfield", "IL", "62701", "USA");
        var billingAddress = new OrderAddress("456 Elm St", "Springfield", "IL", "62702", "USA");
        var command = new PlaceOrder(customerId, shippingAddress, billingAddress);

        var customer = new CustomerSnapshotReadModel
        {
            Id = customerId,
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            LastUpdated = DateTime.UtcNow
        };
        _session.LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var orderId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().NotBeEmpty();
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _session.Received(1).LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ThrowsCustomerNotFoundException()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var shippingAddress = new OrderAddress("123 Main St", "Springfield", "IL", "62701", "USA");
        var billingAddress = new OrderAddress("456 Elm St", "Springfield", "IL", "62702", "USA");
        var command = new PlaceOrder(customerId, shippingAddress, billingAddress);

        _session.LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns((CustomerSnapshotReadModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomerNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.CustomerId.Should().Be(customerId);
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomerNotYetReplicated_ThrowsCustomerNotFoundException()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var shippingAddress = new OrderAddress("123 Main St", "Springfield", "IL", "62701", "USA");
        var billingAddress = new OrderAddress("456 Elm St", "Springfield", "IL", "62702", "USA");
        var command = new PlaceOrder(customerId, shippingAddress, billingAddress);

        // Customer hasn't been replicated yet (eventual consistency gap)
        _session.LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns((CustomerSnapshotReadModel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<CustomerNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_EmptyCustomerId_ThrowsArgumentException()
    {
        // Arrange
        var shippingAddress = new OrderAddress("123 Main St", "Springfield", "IL", "62701", "USA");
        var billingAddress = new OrderAddress("456 Elm St", "Springfield", "IL", "62702", "USA");
        var command = new PlaceOrder(Guid.Empty, shippingAddress, billingAddress);

        var customer = new CustomerSnapshotReadModel
        {
            Id = Guid.Empty,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            LastUpdated = DateTime.UtcNow
        };
        _session.LoadAsync<CustomerSnapshotReadModel>(Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCustomerSnapshot()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var shippingAddress = new OrderAddress("123 Main St", "Springfield", "IL", "62701", "USA");
        var billingAddress = new OrderAddress("456 Elm St", "Springfield", "IL", "62702", "USA");
        var command = new PlaceOrder(customerId, shippingAddress, billingAddress);

        var customer = new CustomerSnapshotReadModel
        {
            Id = customerId,
            Email = "jane.smith@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            LastUpdated = DateTime.UtcNow
        };
        _session.LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - verify customer snapshot was loaded from local read model
        await _session.Received(1).LoadAsync<CustomerSnapshotReadModel>(customerId, Arg.Any<CancellationToken>());

        // Verify StartStream was called with customer snapshot
        _session.Events.Received(1).StartStream<OrderAggregate>(
            Arg.Any<Guid>(),
            Arg.Is<IEnumerable<object>>(events => VerifyCustomerSnapshot(events, "Jane Smith", "j***@example.com")));
    }

    private static bool VerifyCustomerSnapshot(IEnumerable<object> events, string expectedName, string expectedEmail)
    {
        var placedEvent = events.OfType<Domain.DomainEvents.OrderPlaced>().FirstOrDefault();
        return placedEvent?.CustomerSnapshot != null &&
               placedEvent.CustomerSnapshot.CustomerName == expectedName &&
               placedEvent.CustomerSnapshot.MaskedEmail == expectedEmail;
    }
}
