using MediatR;

namespace CookieCommerce.AuthenticationManagement.Application.Auth.Commands;

public sealed record ConsumeCustomerProfileUpdated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CustomerId,
    string FirstName,
    string LastName,
    string? Gender,
    DateOnly? DateOfBirth) : IRequest;
