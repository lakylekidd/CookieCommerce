using CookieCommerce.AuthDomain.Application.Auth.ReadModels;
using MediatR;

namespace CookieCommerce.AuthDomain.Application.Auth.Commands;

public record LoginUser(string Email, string Password) : IRequest<LoginResult>;
