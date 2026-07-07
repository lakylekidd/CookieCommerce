using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

public sealed class ConsumeCustomerForgottenCommandHandler(
    IDocumentSession session,
    IKeycloakAdminService keycloakAdmin)
    : IRequestHandler<ConsumeCustomerForgotten>
{
    public async Task Handle(ConsumeCustomerForgotten request, CancellationToken cancellationToken)
    {
        var authUser = await session.Events.AggregateStreamAsync<AuthUserAggregate>(
            request.CustomerId,
            token: cancellationToken);

        if (authUser is null || !authUser.IsActive)
            return;

        authUser.Forget(request.OccurredAtUtc, request.Reason);
        session.Events.Append(request.CustomerId, authUser.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        await keycloakAdmin.DisableUserAsync(authUser.KeycloakId, cancellationToken);
    }
}
