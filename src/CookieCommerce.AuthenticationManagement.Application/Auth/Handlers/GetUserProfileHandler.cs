using CookieCommerce.AuthenticationManagement.Application.Auth.Queries;
using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using CookieCommerce.AuthenticationManagement.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Handlers;

public class GetUserProfileHandler(IQuerySession session) : IRequestHandler<GetUserProfile, AuthUserReadModel>
{
    public async Task<AuthUserReadModel> Handle(GetUserProfile query, CancellationToken cancellationToken)
    {
        var user = await session.LoadAsync<AuthUserReadModel>(query.UserId, cancellationToken);
        if (user is null)
        {
            var identityId = query.UserId.ToString();
            user = await session.Query<AuthUserReadModel>().Where(x => x.CustomerId == query.UserId || x.KeycloakId == identityId).FirstOrDefaultAsync(cancellationToken);
        }
        if (user is null)
            throw new AuthUserNotFoundException(query.UserId);
        return user;
    }
}
