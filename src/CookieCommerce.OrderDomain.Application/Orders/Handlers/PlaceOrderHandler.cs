using CookieCommerce.OrderDomain.Application.Customers;
using CookieCommerce.OrderDomain.Application.Orders.Commands;
using CookieCommerce.OrderDomain.Domain.Aggregates;
using CookieCommerce.OrderDomain.Domain.Exceptions;
using CookieCommerce.OrderDomain.Domain.ValueObjects;
using Marten;
using MediatR;

namespace CookieCommerce.OrderDomain.Application.Orders.Handlers;

/// <summary>
/// Handler for placing a new order.
/// Validates that the customer exists before creating the order.
/// </summary>
public class PlaceOrderHandler(
    IDocumentSession session, 
    ICustomerService customerService) : IRequestHandler<PlaceOrder, Guid>
{
    public async Task<Guid> Handle(PlaceOrder request, CancellationToken cancellationToken)
    {
        // Validate customer exists
        var customer = await customerService.GetCustomerByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
            throw new CustomerNotFoundException(request.CustomerId);

        // Create customer snapshot with masked/non-PII data
        var customerSnapshot = new CustomerSnapshot(
            customer.Id,
            $"{customer.FirstName} {customer.LastName}",
            CustomerHelpers.MaskEmail(customer.Email)
        );

        var orderId = Guid.CreateVersion7();

        var order = new OrderAggregate();
        order.PlaceOrder(orderId, request.CustomerId, customerSnapshot);

        session.Events.StartStream<OrderAggregate>(orderId, order.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        return orderId;
    }
}
