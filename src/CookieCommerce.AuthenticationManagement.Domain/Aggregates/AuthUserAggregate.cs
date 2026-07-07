using CookieCommerce.AuthenticationManagement.Domain.DomainEvents;
using CookieCommerce.AuthenticationManagement.Domain.Exceptions;
using CookieCommerce.DomainBuildingBlocks;

namespace CookieCommerce.AuthenticationManagement.Domain.Aggregates;

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

    public void ChangeVerifiedEmail(string verifiedEmail, DateTimeOffset changedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(verifiedEmail))
            throw new InvalidCredentialsException("Verified email is required.");

        if (string.Equals(Email, verifiedEmail, StringComparison.OrdinalIgnoreCase))
            return;

        Apply(new AuthUserEmailChanged
        {
            UserId = Id,
            PreviousEmail = Email,
            VerifiedEmail = verifiedEmail,
            ChangedAtUtc = changedAtUtc
        });
    }

    public bool ChangeProfile(
        string firstName,
        string lastName,
        string? gender,
        DateOnly? dateOfBirth,
        DateTimeOffset changedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidCredentialsException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new InvalidCredentialsException("Last name is required.");

        if (string.Equals(FirstName, firstName, StringComparison.Ordinal)
            && string.Equals(LastName, lastName, StringComparison.Ordinal))
            return false;

        Apply(new AuthUserProfileChanged
        {
            UserId = Id,
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            ChangedAtUtc = changedAtUtc
        });

        return true;
    }

    public void Forget(DateTimeOffset forgottenAtUtc, string reason)
    {
        if (!IsActive)
            return;

        Apply(new AuthUserForgotten
        {
            UserId = Id,
            ForgottenAtUtc = forgottenAtUtc,
            Reason = reason
        });
    }

    protected override void When(object @event)
    {
        switch (@event)
        {
            case AuthUserRegistered e: Handle(e); break;
            case AuthUserEmailChanged e: Handle(e); break;
            case AuthUserProfileChanged e: Handle(e); break;
            case AuthUserForgotten e: Handle(e); break;
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

    private void Handle(AuthUserEmailChanged e)
    {
        Email = e.VerifiedEmail;
    }

    private void Handle(AuthUserProfileChanged e)
    {
        FirstName = e.FirstName;
        LastName = e.LastName;
    }

    private void Handle(AuthUserForgotten e)
    {
        IsActive = false;
    }
}
