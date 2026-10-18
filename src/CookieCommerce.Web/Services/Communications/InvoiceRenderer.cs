using System.Globalization;
using System.Net;
using System.Text;
using PdfSharp;
using PdfSharp.Fonts;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace CookieCommerce.Web.Services.Communications;

public sealed class InvoiceRenderer(IWebHostEnvironment environment)
{
    private static readonly object FontGate = new();

    public byte[] ReadLogo(string path)
    {
        var root = Path.GetFullPath(Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "images"));
        var file = Path.GetFullPath(Path.Combine(root, path["/images/".Length..]));
        if (!file.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid invoice logo path.");
        return File.Exists(file) && new FileInfo(file).Length <= 2 * 1024 * 1024 ? File.ReadAllBytes(file) : [];
    }

    public string RenderHtml(InvoiceRecord invoice)
    {
        lock (FontGate) GlobalFontSettings.FontResolver ??= new InvoiceFontResolver();
        var values = InvoiceGenerator.Values(invoice);
        var html = string.IsNullOrWhiteSpace(invoice.Template.HtmlBody) ? TemplateService.DefaultInvoiceHtml : invoice.Template.HtmlBody;
        html = TemplateService.Replace(html, values, true)
            .Replace("__INVOICE_ACCENT__", SafeAccent(invoice.Template.AccentColor), StringComparison.Ordinal)
            .Replace("__INVOICE_LOGO__", LogoHtml(invoice.LogoBytes), StringComparison.Ordinal)
            .Replace("__INVOICE_HEADING__", TextHtml(TemplateService.Replace(invoice.Template.Heading, values, false)), StringComparison.Ordinal)
            .Replace("__INVOICE_COMPANY__", CompanyHtml(invoice.Company), StringComparison.Ordinal)
            .Replace("__INVOICE_ADDRESS__", AddressHtml(invoice), StringComparison.Ordinal)
            .Replace("__INVOICE_ITEMS__", ItemsHtml(invoice), StringComparison.Ordinal)
            .Replace("__INVOICE_TOTALS__", TotalsHtml(invoice), StringComparison.Ordinal)
            .Replace("__INVOICE_FOOTER__", TextHtml(TemplateService.Replace(invoice.Template.Footer, values, false)), StringComparison.Ordinal);
        return html;
    }

    public byte[] Render(InvoiceRecord invoice)
    {
        var pdf = PdfGenerator.GeneratePdf(RenderHtml(invoice), PageSize.A4);
        using var output = new MemoryStream();
        pdf.Save(output, false);
        return output.ToArray();
    }

    private static string LogoHtml(byte[] bytes) => bytes.Length == 0 ? "" : $"<img class=\"invoice-logo\" src=\"data:image/png;base64,{Convert.ToBase64String(bytes)}\" alt=\"Shop logo\" />";

    private static string CompanyHtml(InvoiceSettings company)
    {
        var content = new StringBuilder($"<strong>{TextHtml(company.CompanyName)}</strong>");
        if (!string.IsNullOrWhiteSpace(company.CompanyAddress)) content.Append("<br />").Append(TextHtml(company.CompanyAddress));
        if (!string.IsNullOrWhiteSpace(company.CompanyRegistration)) content.Append("<br />Registration: ").Append(TextHtml(company.CompanyRegistration));
        if (!string.IsNullOrWhiteSpace(company.VatNumber)) content.Append("<br />VAT: ").Append(TextHtml(company.VatNumber));
        return content.ToString();
    }

    private static string AddressHtml(InvoiceRecord invoice)
    {
        var address = invoice.Order.BillingAddress;
        return string.Join("<br />", new[]
        {
            TextHtml($"{address.Street} {address.HouseNumber}{address.HouseNumberAddition}"),
            TextHtml($"{address.PostalCode} {address.City}"),
            TextHtml(address.Country)
        });
    }

    private static string ItemsHtml(InvoiceRecord invoice)
    {
        var rows = new StringBuilder();
        foreach (var line in invoice.Order.Products)
            rows.Append("<tr><td>").Append(TextHtml(line.ProductName + "\n" + line.ProductVariantName).Replace("\n", "<br />", StringComparison.Ordinal))
                .Append("</td><td>").Append(line.Quantity.ToString(CultureInfo.InvariantCulture)).Append("</td><td>EUR ")
                .Append(Money(line.UnitPrice)).Append("</td><td>EUR ").Append(Money(line.UnitPrice * line.Quantity)).Append("</td></tr>");
        return rows.ToString();
    }

    private static string TotalsHtml(InvoiceRecord invoice)
    {
        var rows = new StringBuilder();
        Total(rows, "Items", invoice.Order.SubTotalPrice);
        Total(rows, "Shipping", invoice.Order.Shipping);
        Total(rows, "Promotion discount", -invoice.Order.GetPromotionDiscount());
        Total(rows, "Reward discount", -invoice.Order.RewardDiscount);
        Total(rows, "Total excluding VAT", invoice.NetAmount);
        Total(rows, $"VAT {invoice.Order.TaxRate * 100:0.##}%", invoice.TaxAmount);
        rows.Append("<tr class=\"invoice-total\"><td colspan=\"1\">Total paid</td><td colspan=\"1\">EUR ").Append(Money(invoice.Order.Total)).Append("</td></tr>");
        return rows.ToString();
    }

    private static void Total(StringBuilder rows, string label, decimal amount) => rows.Append("<tr><td>").Append(TextHtml(label)).Append("</td><td>EUR ").Append(Money(amount)).Append("</td></tr>");

    private static string TextHtml(string value) => WebUtility.HtmlEncode(value).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "<br />", StringComparison.Ordinal);
    private static string Money(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);
    private static string SafeAccent(string value) => System.Text.RegularExpressions.Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$") ? value : "#D97706";

    private sealed class InvoiceFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic) => new(bold ? "NotoSans-Bold" : "NotoSans-Regular", false, italic);
        public byte[] GetFont(string faceName)
        {
            var assembly = typeof(InvoiceRenderer).Assembly;
            var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith(faceName + ".ttf", StringComparison.Ordinal));
            using var input = assembly.GetManifestResourceStream(name)!;
            using var output = new MemoryStream();
            input.CopyTo(output);
            return output.ToArray();
        }
    }
}
