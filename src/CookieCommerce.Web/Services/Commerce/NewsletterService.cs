using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CookieCommerce.Web.Models;
using Microsoft.Extensions.Options;

namespace CookieCommerce.Web.Services.Commerce;

public sealed class NewsletterSubscription
{
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTimeOffset SubscribedAt { get; set; }
}

public sealed class NewsletterService(ICommerceStore store, INewsletterPublisher? publisher = null) : INewsletterService
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

        if (publisher is not null)
            await publisher.PublishAsync(email.Trim(), name.Trim());

        return ResultDto.Ok("Thank you for subscribing!");
    }
}

public interface INewsletterPublisher
{
    Task PublishAsync(string email, string name, CancellationToken cancellationToken = default);
}

public sealed class ListmonkOptions
{
    public string BaseUrl { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int ListId { get; set; } = 1;
}

public sealed class ListmonkNewsletterPublisher(
    HttpClient client,
    IOptions<ListmonkOptions> options,
    ILogger<ListmonkNewsletterPublisher> logger) : INewsletterPublisher
{
    private readonly ListmonkOptions options = options.Value;

    public async Task PublishAsync(string email, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return;

        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.Username}:{options.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        using var response = await client.PostAsJsonAsync("api/subscribers", new
        {
            email,
            name,
            lists = new[] { options.ListId },
            preconfirm_subscriptions = true
        }, cancellationToken);

        if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogWarning("Listmonk rejected newsletter subscription with status {StatusCode}: {ResponseBody}", response.StatusCode, body[..Math.Min(body.Length, 200)]);
        response.EnsureSuccessStatusCode();
    }
}
