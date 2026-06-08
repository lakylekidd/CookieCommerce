using CookieCommerce.AuthDomain.Application.Auth.Queries;
using CookieCommerce.AuthDomain.Application.Auth.ReadModels;
using CookieCommerce.AuthDomain.Domain.Exceptions;
using Marten;
using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Handlers;

public class GetUserProfileHandler(IQuerySession session) : IRequestHandler<GetUserProfile, AuthUserReadModel>
{
    public async Task<AuthUserReadModel> Handle(GetUserProfile query, CancellationToken cancellationToken)
    {
        var user = await session.LoadAsync<AuthUserReadModel>(query.UserId, cancellationToken);
        if (user is null)
            throw new AuthUserNotFoundException(query.UserId);
        return user;
    }
}
