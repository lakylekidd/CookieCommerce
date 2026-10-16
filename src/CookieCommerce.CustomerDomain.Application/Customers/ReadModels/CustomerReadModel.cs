using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Application.Customers.ReadModels;

public class CustomerReadModel
{
    public DateTimeOffset? CreatedAtUtc { get; set; }
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? PendingEmail { get; set; }
    public string EmailVerificationStatus { get; set; } = "Verified";
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public CustomerProfileGender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public List<CustomerAddressReadModel> Addresses { get; set; } = new();
}
