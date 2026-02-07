using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public class CreateCustomerRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Unknown;
    public DateTime? DateOfBirth { get; set; } = null;
}
