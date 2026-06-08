using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Commands;

public record RegisterUser(string Email, string Password, string FirstName, string LastName) : IRequest<Guid>;
