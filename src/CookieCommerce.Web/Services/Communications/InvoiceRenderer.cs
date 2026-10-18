using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

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

    public byte[] Render(InvoiceRecord invoice)
    {
        lock (FontGate) GlobalFontSettings.FontResolver ??= new InvoiceFontResolver();
        var document = new Document();
        document.Info.Title = invoice.Number;
        document.Info.Author = invoice.Company.CompanyName;
        document.Styles[StyleNames.Normal]!.Font.Name = "CookieCommerce";
        document.Styles[StyleNames.Normal]!.Font.Size = 10;
        document.Styles[StyleNames.Normal]!.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);
        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
        section.PageSetup.LeftMargin = section.PageSetup.RightMargin = Unit.FromCentimeter(2);
        if (invoice.IsDevelopmentSeed)
        {
            var sample = section.AddParagraph("DEVELOPMENT SAMPLE — no payment was processed");
            sample.Format.Font.Bold = true; sample.Format.Font.Color = Colors.DarkRed;
        }
        if (invoice.LogoBytes.Length > 0)
        {
            var logo = section.AddImage("base64:" + Convert.ToBase64String(invoice.LogoBytes));
            logo.Width = Unit.FromCentimeter(3); logo.LockAspectRatio = true;
        }
        var accent = Color.Parse(invoice.Template.AccentColor);
        var shopHeader = Color.Parse("#451A03");
        var title = section.AddParagraph(TemplateService.Replace(invoice.Template.Heading, InvoiceGenerator.Values(invoice), false));
        title.Format.Font.Size = 24; title.Format.Font.Bold = true;
        title.Format.Font.Color = accent;
        var companyName = section.AddParagraph(invoice.Company.CompanyName);
        companyName.Format.Font.Bold = true; companyName.Format.Font.Color = shopHeader;
        if (invoice.Company.CompanyAddress.Length > 0) section.AddParagraph(invoice.Company.CompanyAddress);
        if (invoice.Company.CompanyRegistration.Length > 0) section.AddParagraph("Registration: " + invoice.Company.CompanyRegistration);
        if (invoice.Company.VatNumber.Length > 0) section.AddParagraph("VAT: " + invoice.Company.VatNumber);
        section.AddParagraph($"Issued: {TimeZoneInfo.ConvertTimeBySystemTimeZoneId(invoice.IssuedAt, "Europe/Amsterdam"):yyyy-MM-dd}    Order: {invoice.Order.OrderReference}");
        var address = invoice.Order.BillingAddress;
        section.AddParagraph($"Bill to\n{address.FirstName} {address.LastName}\n{address.Street} {address.HouseNumber}{address.HouseNumberAddition}\n{address.PostalCode} {address.City}\n{address.Country}");
        var table = section.AddTable();
        table.Borders.Color = accent; table.Borders.Width = 0.5;
        foreach (var width in new[] { 9.3, 1.8, 2.8, 3.1 }) table.AddColumn(Unit.FromCentimeter(width));
        var header = table.AddRow(); header.HeadingFormat = true; header.Format.Font.Bold = true;
        header.Shading.Color = accent; header.Format.Font.Color = Colors.White;
        header.Cells[0].AddParagraph("Item"); header.Cells[1].AddParagraph("Qty"); header.Cells[2].AddParagraph("Unit EUR"); header.Cells[3].AddParagraph("Total EUR");
        foreach (var line in invoice.Order.Products)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(line.ProductName + "\n" + line.ProductVariantName);
            row.Cells[1].AddParagraph(line.Quantity.ToString(CultureInfo.InvariantCulture));
            row.Cells[2].AddParagraph(Money(line.UnitPrice)); row.Cells[3].AddParagraph(Money(line.UnitPrice * line.Quantity));
            for (var c = 1; c < 4; c++) row.Cells[c].Format.Alignment = ParagraphAlignment.Right;
        }
        section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(10);
        var totals = section.AddTable(); totals.AddColumn(Unit.FromCentimeter(12)); totals.AddColumn(Unit.FromCentimeter(5));
        void Total(string label, decimal amount, bool bold = false)
        {
            var row = totals.AddRow(); row.Format.Font.Bold = bold;
            row.Cells[0].AddParagraph(label); row.Cells[1].AddParagraph("EUR " + Money(amount)); row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
        }
        Total("Items", invoice.Order.SubTotalPrice); Total("Shipping", invoice.Order.Shipping);
        Total("Promotion discount", -invoice.Order.GetPromotionDiscount()); Total("Reward discount", -invoice.Order.RewardDiscount);
        Total("Total excluding VAT", invoice.NetAmount); Total($"VAT {invoice.Order.TaxRate * 100:0.##}%", invoice.TaxAmount);
        Total("Total paid", invoice.Order.Total, true);
        var totalRow = totals.Rows[totals.Rows.Count - 1];
        totalRow.Shading.Color = accent; totalRow.Format.Font.Color = Colors.White;
        var footerText = section.AddParagraph(TemplateService.Replace(invoice.Template.Footer, InvoiceGenerator.Values(invoice), false));
        footerText.Format.SpaceBefore = Unit.FromPoint(15); footerText.Format.Font.Color = shopHeader;
        var footer = section.Footers.Primary.AddParagraph(invoice.Number + " · Page ");
        footer.AddPageField(); footer.AddText(" of "); footer.AddNumPagesField(); footer.Format.Alignment = ParagraphAlignment.Center; footer.Format.Font.Color = accent;
        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();
        using var output = new MemoryStream(); renderer.PdfDocument.Save(output, false); return output.ToArray();
    }

    private static string Money(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

    private sealed class InvoiceFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic) => new(bold ? "NotoSans-Bold" : "NotoSans-Regular", false, italic);
        public byte[] GetFont(string faceName)
        {
            var assembly = typeof(InvoiceRenderer).Assembly;
            var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith(faceName + ".ttf", StringComparison.Ordinal));
            using var input = assembly.GetManifestResourceStream(name)!; using var output = new MemoryStream(); input.CopyTo(output); return output.ToArray();
        }
    }
}
