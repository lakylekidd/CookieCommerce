using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using CookieCommerce.AuthenticationManagement.Domain.DomainEvents;
using Marten.Events.Aggregation;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Projections;

public class AuthUserProjection : SingleStreamProjection<AuthUserReadModel, Guid>
{
    public AuthUserReadModel Create(AuthUserRegistered e) => new AuthUserReadModel
    {
        Id = e.UserId,
        KeycloakId = e.KeycloakId,
        Email = e.Email,
        FirstName = e.FirstName,
        LastName = e.LastName,
        IsActive = true,
        CreatedAt = e.RegisteredAt
    };

    public void Apply(AuthUserEmailChanged e, AuthUserReadModel current)
    {
        current.Email = e.VerifiedEmail;
    }

    public void Apply(AuthUserProfileChanged e, AuthUserReadModel current)
    {
        current.FirstName = e.FirstName;
        current.LastName = e.LastName;
    }

    public void Apply(AuthUserForgotten e, AuthUserReadModel current)
    {
        current.IsActive = false;
    }
}
