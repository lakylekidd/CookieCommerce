namespace CookieCommerce.Web.Services;

public interface IContactService
{
    Task<bool> SendContactEmailAsync(string fromEmail, string subject, string message);
}
