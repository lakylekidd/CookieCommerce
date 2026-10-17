using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using CookieCommerce.Web.Models;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class NewsletterSubscription
{
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTimeOffset SubscribedAt { get; set; }
}

public sealed class NewsletterService(ICommerceStore store) : INewsletterService
{
    public static string Key(string email) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant())));
    public async Task<ResultDto> SubscribeAsync(string email, string name)
    {
        if (!new EmailAddressAttribute().IsValid(email) || email.Length > 254 || name.Length > 200) return ResultDto.Fail("Enter a valid email address and name.");
        var key = Key(email);
        await store.ExecuteAsync($"newsletter:{key}", async tx =>
        {
            if (await tx.LoadAsync<NewsletterSubscription>(key) is null)
                await tx.SaveAsync(key, new NewsletterSubscription { Email = email.Trim(), Name = name.Trim(), SubscribedAt = DateTimeOffset.UtcNow });
            return true;
        });
        return ResultDto.Ok("Thank you for subscribing!");
    }
}
