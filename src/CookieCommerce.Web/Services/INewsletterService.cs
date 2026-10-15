namespace CookieCommerce.Web.Services;

/// <summary>
/// The newsletter service interface defines the contract for managing newsletter subscriptions.
/// </summary>
public interface INewsletterService
{
    Task<Models.ResultDto> SubscribeAsync(string email, string name);
}
