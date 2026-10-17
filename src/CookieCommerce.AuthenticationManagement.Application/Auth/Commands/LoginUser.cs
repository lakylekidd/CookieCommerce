using CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;
using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Commands;

public record LoginUser(string Email, string Password) : IRequest<LoginResult>;
