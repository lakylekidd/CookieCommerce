using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Commands;

public record LogoutUser(string RefreshToken) : IRequest;
