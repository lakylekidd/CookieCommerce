using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Commands;

public sealed record ConsumeCustomerForgotten(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string? IdentitySubject,
    string Reason) : IRequest;
