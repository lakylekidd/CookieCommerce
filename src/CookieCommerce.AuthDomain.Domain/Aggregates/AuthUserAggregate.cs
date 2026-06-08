using CookieCommerce.AuthDomain.Domain.DomainEvents;
using CookieCommerce.AuthDomain.Domain.Exceptions;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.AuthDomain.Domain.Aggregates;

public class AuthUserAggregate : AggregateBase
{
    public string KeycloakId { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public AuthUserAggregate() { }

    public void Register(Guid userId, string keycloakId, string email, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidCredentialsException("Email is required.");
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new InvalidCredentialsException("Keycloak ID is required.");

        Apply(new AuthUserRegistered
        {
            UserId = userId,
            KeycloakId = keycloakId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            RegisteredAt = DateTime.UtcNow
        });
    }

    public void RecordLogin(string sessionId)
    {
        Apply(new AuthUserLoggedIn
        {
            UserId = Id,
            SessionId = sessionId,
            LoggedInAt = DateTime.UtcNow
        });
    }

    public void RecordLogout(string sessionId)
    {
        Apply(new AuthUserLoggedOut
        {
            UserId = Id,
            SessionId = sessionId,
            LoggedOutAt = DateTime.UtcNow
        });
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case AuthUserRegistered e: Handle(e); break;
            case AuthUserLoggedIn: break;
            case AuthUserLoggedOut: break;
        }
    }

    private void Handle(AuthUserRegistered e)
    {
        Id = e.UserId;
        KeycloakId = e.KeycloakId;
        Email = e.Email;
        FirstName = e.FirstName;
        LastName = e.LastName;
        IsActive = true;
        RegisteredAt = e.RegisteredAt;
    }
}
