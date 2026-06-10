using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Queries;

public record GetUserProfile(Guid UserId) : IRequest<AuthUserReadModel>;
