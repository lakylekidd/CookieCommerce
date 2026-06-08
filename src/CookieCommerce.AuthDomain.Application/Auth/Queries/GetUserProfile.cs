using CookieCommerce.AuthDomain.Application.Auth.ReadModels;
using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Queries;

public record GetUserProfile(Guid UserId) : IRequest<AuthUserReadModel>;
