using System.ComponentModel.DataAnnotations;

namespace CookieCommerce.CustomerDomain.Api.Models.Customers;

public class AddCustomerAddressRequest
{
    [Required] public string Label { get; set; } = string.Empty;
    [Required] public string Street { get; set; } = string.Empty;
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string State { get; set; } = string.Empty;
    [Required] public string PostalCode { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
}
