using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Commands;

public record RegisterUser(string Email, string Password, string FirstName, string LastName) : IRequest<Guid>;
