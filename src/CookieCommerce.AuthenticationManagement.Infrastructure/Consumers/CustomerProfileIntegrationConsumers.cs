using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Consumers;

public sealed class CustomerProfileUpdatedConsumer(
    ISender sender,
    ILogger<CustomerProfileUpdatedConsumer> logger)
    : IConsumer<CustomerProfileUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomerProfileUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        await sender.Send(
            new ConsumeCustomerProfileUpdated(
                message.EventId,
                message.OccurredAtUtc,
                message.CustomerId,
                message.FirstName,
                message.LastName,
                message.Gender,
                message.DateOfBirth),
            context.CancellationToken);

        logger.LogInformation(
            "Consumed customer profile update for customer {CustomerId} [eventId={EventId}]",
            message.CustomerId,
            message.EventId);
    }
}

public sealed class VerifiedCustomerEmailChangedConsumer(
    ISender sender,
    ILogger<VerifiedCustomerEmailChangedConsumer> logger)
    : IConsumer<VerifiedCustomerEmailChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<VerifiedCustomerEmailChangedIntegrationEvent> context)
    {
        var message = context.Message;

        await sender.Send(
            new ConsumeVerifiedCustomerEmailChanged(
                message.EventId,
                message.OccurredAtUtc,
                message.CustomerId,
                message.PreviousEmail,
                message.VerifiedEmail),
            context.CancellationToken);

        logger.LogInformation(
            "Consumed verified customer email change for customer {CustomerId} [eventId={EventId}]",
            message.CustomerId,
            message.EventId);
    }
}

public sealed class CustomerForgottenConsumer(
    ISender sender,
    ILogger<CustomerForgottenConsumer> logger)
    : IConsumer<CustomerForgottenIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomerForgottenIntegrationEvent> context)
    {
        var message = context.Message;

        await sender.Send(
            new ConsumeCustomerForgotten(
                message.EventId,
                message.OccurredAtUtc,
                message.CustomerId,
                message.IdentitySubject,
                message.Reason),
            context.CancellationToken);

        logger.LogInformation(
            "Consumed customer forgotten event for customer {CustomerId} [eventId={EventId}]",
            message.CustomerId,
            message.EventId);
    }
}
