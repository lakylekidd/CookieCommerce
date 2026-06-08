using CookieCommerce.AuthDomain.Application.Auth.ReadModels;
using CookieCommerce.AuthDomain.Domain.DomainEvents;
using Marten.Events.Aggregation;

namespace CookieCommerce.AuthDomain.Infrastructure.Projections;

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
}
