using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Commands;

public sealed record ConsumeVerifiedCustomerEmailChanged(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string PreviousEmail,
    string VerifiedEmail) : IRequest;
