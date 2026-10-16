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
        var authUser = await AuthCustomerIdentityResolver.ResolveAsync(session, request.CustomerId, cancellationToken);

        if (authUser is null || !authUser.IsActive)
            return;

        await keycloakAdmin.DisableUserAsync(authUser.KeycloakId, cancellationToken);
        authUser.Forget(request.OccurredAtUtc, request.Reason);
        session.Events.Append(authUser.Id, authUser.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);
    }
}
