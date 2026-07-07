using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

public sealed class ConsumeVerifiedCustomerEmailChangedCommandHandler(
    IDocumentSession session,
    IKeycloakAdminService keycloakAdmin)
    : IRequestHandler<ConsumeVerifiedCustomerEmailChanged>
{
    public async Task Handle(ConsumeVerifiedCustomerEmailChanged request, CancellationToken cancellationToken)
    {
        var authUser = await session.Events.AggregateStreamAsync<AuthUserAggregate>(
            request.CustomerId,
            token: cancellationToken);

        if (authUser is null || !authUser.IsActive)
            return;

        if (string.Equals(authUser.Email, request.VerifiedEmail, StringComparison.OrdinalIgnoreCase))
            return;

        authUser.ChangeVerifiedEmail(request.VerifiedEmail, request.OccurredAtUtc);
        session.Events.Append(request.CustomerId, authUser.DomainEvents);
        await session.SaveChangesAsync(cancellationToken);

        await keycloakAdmin.UpdateUserEmailAsync(authUser.KeycloakId, request.VerifiedEmail, cancellationToken);
    }
}
