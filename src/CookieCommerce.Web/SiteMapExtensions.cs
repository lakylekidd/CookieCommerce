using System.Xml.Linq;
using CookieCommerce.Web.Services.Payments;
using CookieCommerce.Web.Services.Products;
using Microsoft.Extensions.Options;

namespace CookieCommerce.Web;

public static class SiteMapExtensions
{
    public static void AddSiteMapOptions(this WebApplication app)
    {
        app.MapGet("/sitemap.xml", async (HttpContext context, ICookieService catalog, IOptions<PaymentOptions> payments) =>
        {
            var origin = PublicOrigin(context, payments.Value);
            var pages = new[] { "/", "/shop", "/our-story", "/legal/faq", "/legal/terms", "/legal/privacy", "/legal/cookies", "/legal/refund" };
            var products = await catalog.GetFavoriteCookiesAsync(context.RequestAborted);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urls = pages.Concat(products.Select(p => $"/shop/cookie/{p.Id}")).Distinct();
            var document = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "urlset", urls.Select(path => new XElement(ns + "url", new XElement(ns + "loc", new Uri(origin, path).AbsoluteUri)))));
            return Results.Content(document.ToString(), "application/xml");
        });
    }

    public static void AddRobots(this WebApplication app)
    {
        app.MapGet("/robots.txt", (HttpContext context, IOptions<PaymentOptions> payments) =>
            Results.Text($"User-agent: *\nAllow: /\nDisallow: /account\nDisallow: /backoffice\nDisallow: /auth\nDisallow: /design-system\nDisallow: /shop/checkout\nSitemap: {new Uri(PublicOrigin(context, payments.Value), "/sitemap.xml").AbsoluteUri}\n"));
    }

    private static Uri PublicOrigin(HttpContext context, PaymentOptions payments) =>
        Uri.TryCreate(payments.PublicBaseUrl, UriKind.Absolute, out var configured) ? configured : new Uri($"{context.Request.Scheme}://{context.Request.Host}");
}
