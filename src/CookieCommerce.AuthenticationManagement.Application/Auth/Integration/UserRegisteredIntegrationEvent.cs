namespace CookieCommerce.AuthenticationManagement.Application.Auth.Integration;

/// <summary>
/// Published by <see cref="Handlers.RegisterUserHandler"/> after a user is successfully
/// created in both Keycloak and the auth event store.
/// Downstream bounded contexts (e.g. CustomerDomain) consume this event to initialise
/// their own representation of the user.
/// </summary>
public sealed class UserRegisteredIntegrationEvent
{
    public Guid UserId { get; init; }

    /// <summary>
    /// The Keycloak-native user UUID. This is the JWT <c>sub</c> claim that downstream
    /// contexts (e.g. CustomerDomain) should use as their own record ID so that the
    /// identity is consistent with whatever the token carries.
    /// </summary>
    public string KeycloakId { get; init; } = default!;

    public string Email { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public DateTime RegisteredAt { get; init; }
}
