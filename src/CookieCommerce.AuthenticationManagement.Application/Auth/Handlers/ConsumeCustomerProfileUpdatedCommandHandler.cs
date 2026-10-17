using CookieCommerce.AuthenticationManagement.Application.Auth.Commands;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using Marten;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

public sealed class ConsumeCustomerProfileUpdatedCommandHandler(
    IDocumentSession session,
    IKeycloakAdminService keycloakAdmin)
    : IRequestHandler<ConsumeCustomerProfileUpdated>
{
    public async Task Handle(ConsumeCustomerProfileUpdated request, CancellationToken cancellationToken)
    {
        var authUser = await AuthCustomerIdentityResolver.ResolveAsync(session, request.CustomerId, cancellationToken);

        if (authUser is null || !authUser.IsActive)
            return;

        var changed = authUser.ChangeProfile(
            request.FirstName,
            request.LastName,
            request.Gender,
            request.DateOfBirth,
            request.OccurredAtUtc);

        if (changed)
        {
            session.Events.Append(authUser.Id, authUser.DomainEvents);
            await session.SaveChangesAsync(cancellationToken);
        }

        await keycloakAdmin.UpdateUserProfileAsync(
            authUser.KeycloakId,
            request.FirstName,
            request.LastName,
            request.Gender,
            request.DateOfBirth,
            cancellationToken);
    }
}
