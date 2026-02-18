using CookieCommerce.OrderDomain.Application.Customers;
using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Application.Orders.Handlers;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.OrderDomain.Tests.ApplicationTests;

public class PlaceOrderHandlerTests
{
    private readonly IDocumentSession _session;
    private readonly ICustomerService _customerService;
    private readonly PlaceOrderHandler _handler;

    public PlaceOrderHandlerTests()
    {
        _session = Substitute.For<IDocumentSession>();
        _customerService = Substitute.For<ICustomerService>();
        _handler = new PlaceOrderHandler(_session, _customerService);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewOrderId()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var command = new PlaceOrder(customerId);
        
        var customer = new CustomerDto
        {
            Id = customerId,
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        _customerService.GetCustomerByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        var orderId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().NotBeEmpty();
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _customerService.Received(1).GetCustomerByIdAsync(customerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ThrowsCustomerNotFoundException()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var command = new PlaceOrder(customerId);
        
        _customerService.GetCustomerByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((CustomerDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomerNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
        
        exception.CustomerId.Should().Be(customerId);
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomerServiceUnavailable_ThrowsCustomerNotFoundException()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var command = new PlaceOrder(customerId);
        
        // Service returns null when unavailable
        _customerService.GetCustomerByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((CustomerDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<CustomerNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_EmptyCustomerId_ThrowsArgumentException()
    {
        // Arrange
        var command = new PlaceOrder(Guid.Empty);
        
        var customer = new CustomerDto
        {
            Id = Guid.Empty,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        _customerService.GetCustomerByIdAsync(Guid.Empty, Arg.Any<CancellationToken>())
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
        var command = new PlaceOrder(customerId);
        
        var customer = new CustomerDto
        {
            Id = customerId,
            Email = "jane.smith@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };
        _customerService.GetCustomerByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - verify customer service was called
        await _customerService.Received(1).GetCustomerByIdAsync(customerId, Arg.Any<CancellationToken>());
        
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
