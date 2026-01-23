namespace CookieCommerce.Web;

public static class SiteMapExtensions
{
    public static void AddSiteMapOptions(this WebApplication? app)
    {
        // Dynamic Sitemap Endpoint
        app.MapGet("/sitemap.xml", async (HttpContext context) =>
        {
            var host = "https://tigervancookie.nl";

            // TODO: Replace with your dynamic data
            // In the future, you'd fetch your products from a DB here
            // var cookies = await dbContext.Cookies.Select(c => c.Slug).ToListAsync();
            var staticPages = new[] { "" }; // Add other static routes here, like "about", "contact"

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            foreach (var page in staticPages)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{host}/{page}</loc>");
                sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
                sb.AppendLine("    <priority>1.0</priority>");
                sb.AppendLine("  </url>");
            }

            sb.AppendLine("</urlset>");

            return Results.Content(sb.ToString(), "application/xml");
        });
    }

    public static void AddRobots(this WebApplication? app)
    {
        app.MapGet("/robots.txt", () =>
        {
            var robots = "User-agent: *\n" +
                         "Allow: /\n" +
                         $"Sitemap: https://tigervancookie.nl/sitemap.xml";
            return Results.Text(robots);
        });
    }
}
