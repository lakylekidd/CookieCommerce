using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using MassTransit.Topology;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public class CreateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Unknown;
    public DateTime? DateOfBirth { get; set; } = null;
}
