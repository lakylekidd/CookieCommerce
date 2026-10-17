using CookieCommerce.Web.Models.Addresses;

namespace CookieCommerce.Web.Services.Development;

public sealed record DevelopmentSeedCustomer(Guid Id, string Email, string FirstName, string LastName, AddressBookItemDto Address);
