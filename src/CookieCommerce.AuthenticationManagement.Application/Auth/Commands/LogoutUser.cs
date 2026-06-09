using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Commands;

public record LogoutUser(string RefreshToken) : IRequest;
