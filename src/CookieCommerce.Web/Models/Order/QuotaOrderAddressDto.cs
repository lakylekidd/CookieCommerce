namespace CookieCommerce.Web.Models.Order;

public class QuotaOrderAddressDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string GetInitials()
    {
        var firstInitial = !string.IsNullOrEmpty(FirstName) ? FirstName[0].ToString().ToUpper() : "";
        var lastInitial = !string.IsNullOrEmpty(LastName) ? LastName[0].ToString().ToUpper() : "";
        return $"{firstInitial}{lastInitial}";
    }
}
