using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.CustomerDomain.Infrastructure.Consumers;

/// <summary>
/// Consumes <see cref="UserRegisteredIntegrationEvent"/> published by the
/// AuthenticationManagement service and creates the corresponding
/// <see cref="Domain.Aggregates.CustomerAggregate"/> record in the CustomerDomain.
/// This ensures every registered user automatically has a customer profile,
/// whether they came from the bootstrap seed or a live registration flow.
/// </summary>
public sealed class UserRegisteredConsumer(
    ISender mediator,
    ILogger<UserRegisteredConsumer> logger) : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "Creating customer record for newly registered user {UserId} ({Email})",
            @event.UserId, @event.Email);

        await mediator.Send(new CreateCustomer(
            @event.Email,
            @event.FirstName,
            @event.LastName,
            Gender.Unknown,
            DateOfBirth: null,
            CustomerId: Guid.TryParse(@event.KeycloakId, out var keycloakGuid) ? keycloakGuid : null),
            context.CancellationToken);

        logger.LogInformation(
            "Customer record created for user {UserId} ({Email})",
            @event.UserId, @event.Email);
    }
}
