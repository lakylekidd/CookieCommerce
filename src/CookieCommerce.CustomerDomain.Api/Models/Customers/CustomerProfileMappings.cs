using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public static class CustomerProfileMappings
{
    public static CustomerProfileResponse ToResponse(this CustomerProfileResult result, IReadOnlyList<CustomerProfileAddressResponse> addresses)
    {
        return new CustomerProfileResponse(
            Id: result.Id,
            FirstName: result.FirstName,
            LastName: result.LastName,
            Email: result.Email,
            PendingEmail: result.PendingEmail,
            EmailVerificationStatus: result.EmailVerificationStatus.ToString(),
            Gender: result.Gender,
            DateOfBirth: result.DateOfBirth,
            Addresses: addresses);
    }

    public static CustomerProfileResponse ToResponse(this CustomerProfileResult result)
        => result.ToResponse(Array.Empty<CustomerProfileAddressResponse>());

    public static UpdateCustomerProfileCommand ToCommand(
        this UpdateCustomerProfileRequest request,
        Guid customerId,
        string correlationId)
    {
        return new UpdateCustomerProfileCommand(
            CustomerId: customerId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email,
            Gender: request.Gender,
            DateOfBirth: request.DateOfBirth,
            RequestedAtUtc: DateTimeOffset.UtcNow,
            CorrelationId: correlationId);
    }

    public static ResendCustomerEmailVerificationCommand ToCommand(
        this ResendCustomerEmailVerificationRequest request,
        Guid customerId,
        string correlationId)
    {
        return new ResendCustomerEmailVerificationCommand(
            CustomerId: customerId,
            PendingEmail: request.PendingEmail,
            RequestedAtUtc: DateTimeOffset.UtcNow,
            CorrelationId: correlationId);
    }

    public static VerifyCustomerEmailChangeCommand ToCommand(
        this ConfirmCustomerEmailVerificationRequest request,
        Guid customerId,
        string correlationId)
    {
        return new VerifyCustomerEmailChangeCommand(
            CustomerId: customerId,
            Token: request.Token,
            VerifiedAtUtc: DateTimeOffset.UtcNow,
            CorrelationId: correlationId);
    }
}
