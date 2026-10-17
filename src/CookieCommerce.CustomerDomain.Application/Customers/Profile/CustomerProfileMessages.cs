using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using MediatR;

namespace CookieCommerce.CustomerDomain.Application.Customers.Profile;

public sealed record GetCurrentCustomerProfileQuery(Guid CustomerId) : IRequest<CustomerProfileResult>;

public sealed record UpdateCustomerProfileCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string Email,
    CustomerProfileGender? Gender,
    DateOnly? DateOfBirth,
    DateTimeOffset RequestedAtUtc,
    string CorrelationId) : IRequest<CustomerProfileResult>;

public sealed record ResendCustomerEmailVerificationCommand(
    Guid CustomerId,
    string? PendingEmail,
    DateTimeOffset RequestedAtUtc,
    string CorrelationId) : IRequest<EmailVerificationDeliveryResult>;

public sealed record VerifyCustomerEmailChangeCommand(
    Guid CustomerId,
    string Token,
    DateTimeOffset VerifiedAtUtc,
    string CorrelationId) : IRequest<CustomerProfileResult>;

public sealed record CustomerProfileResult(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PendingEmail,
    EmailVerificationStatus EmailVerificationStatus,
    CustomerProfileGender? Gender,
    DateOnly? DateOfBirth);

public sealed record EmailVerificationDeliveryResult(
    EmailVerificationStatus Status,
    string Message,
    string IdempotencyKey);

public enum EmailVerificationStatus
{
    Verified = 0,
    PendingVerification = 1
}
