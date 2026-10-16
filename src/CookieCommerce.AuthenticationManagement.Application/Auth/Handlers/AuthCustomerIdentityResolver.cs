using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using CookieCommerce.AuthenticationManagement.Domain.Aggregates;
using Marten;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

internal static class AuthCustomerIdentityResolver
{
    public static async Task<AuthUserAggregate?> ResolveAsync(IDocumentSession session, Guid customerId, CancellationToken ct)
    {
        var direct = await session.Events.AggregateStreamAsync<AuthUserAggregate>(customerId, token: ct);
        if (direct is not null) { direct.ClearUncommitedEvents(); return direct; }
        var identityId = customerId.ToString();
        var mapping = await session.Query<AuthUserReadModel>().Where(x => x.CustomerId == customerId || x.KeycloakId == identityId).FirstOrDefaultAsync(ct);
        var aggregate = mapping is null ? null : await session.Events.AggregateStreamAsync<AuthUserAggregate>(mapping.Id, token: ct);
        aggregate?.ClearUncommitedEvents();
        return aggregate;
    }
}
