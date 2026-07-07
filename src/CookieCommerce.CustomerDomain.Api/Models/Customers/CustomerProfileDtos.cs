using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public sealed record CustomerProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PendingEmail,
    string EmailVerificationStatus,
    CustomerProfileGender? Gender,
    DateOnly? DateOfBirth,
    IReadOnlyList<CustomerProfileAddressResponse> Addresses);

public sealed record CustomerProfileAddressResponse(
    string Label,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record UpdateCustomerProfileRequest(
    string FirstName,
    string LastName,
    string Email,
    CustomerProfileGender? Gender,
    DateOnly? DateOfBirth);

public sealed record ResendCustomerEmailVerificationRequest(string? PendingEmail);

public sealed record ResendCustomerEmailVerificationResponse(string Status, string Message);

public sealed record ConfirmCustomerEmailVerificationRequest(string Token);
