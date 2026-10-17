using System.Text.Json;
using CookieCommerce.Web.Services.Payments;

namespace CookieCommerce.Web.Services.Commerce;

public static class CommerceEndpoints
{
    public static void MapCommerceEndpoints(this WebApplication app)
    {
        app.MapPost("/api/payments/stripe/webhook", async (HttpRequest request, StripePaymentGateway stripe, PaymentProcessor processor, ILogger<PaymentProcessor> logger) =>
        {
            try
            {
                var size = request.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
                if (size is { IsReadOnly: false }) size.MaxRequestBodySize = 2 * 1024 * 1024;
                using var reader = new StreamReader(request.Body);
                var payload = await reader.ReadToEndAsync(request.HttpContext.RequestAborted);
                var notification = stripe.VerifyWebhook(payload, request.Headers["Stripe-Signature"].ToString(), DateTimeOffset.UtcNow);
                if (notification is not null) await processor.ApplyAsync("Stripe", notification);
                return Results.Ok();
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or JsonException or FormatException or KeyNotFoundException)
            { return Results.BadRequest(); }
            catch (Exception ex)
            {
                logger.LogError("Stripe webhook could not be processed; provider should retry [failureType={FailureType}]", ex.GetType().Name);
                return Results.StatusCode(503);
            }
        }).DisableAntiforgery().WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(2 * 1024 * 1024));

        app.MapPost("/api/payments/mollie/webhook", async (HttpRequest request, MolliePaymentGateway mollie, PaymentProcessor processor, ILogger<PaymentProcessor> logger) =>
        {
            try
            {
                var form = await request.ReadFormAsync();
                var notification = await mollie.RetrieveAsync(form["id"].ToString());
                await processor.ApplyAsync("Mollie", notification);
                return Results.Ok();
            }
            catch (UnauthorizedAccessException) { return Results.BadRequest(); }
            catch (Exception ex)
            {
                logger.LogError("Mollie webhook could not be processed; provider should retry [failureType={FailureType}]", ex.GetType().Name);
                return Results.StatusCode(503);
            }
        }).DisableAntiforgery();

        // Product URLs are relative to the API. Proxy them through the public web origin.
        app.MapGet("/media/products/{**path}", async (string path, HttpContext context, IHttpClientFactory factory) =>
        {
            if (string.IsNullOrWhiteSpace(path) || path.Split('/').Any(p => p is "." or ".."))
            { context.Response.StatusCode = 400; return; }
            using var response = await factory.CreateClient("product-domain-api").GetAsync("media/products/" + path, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
            context.Response.StatusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode) return;
            context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            context.Response.Headers.CacheControl = "public,max-age=3600";
            await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        });
    }
}
