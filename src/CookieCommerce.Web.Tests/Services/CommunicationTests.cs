using System.Text;
using System.Net;
using CookieCommerce.DevBootstrap;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Services.Communications;
using CookieCommerce.Web.Tests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using PdfSharp.Pdf.IO;

namespace CookieCommerce.Web.Tests.Services;

public sealed class CommunicationTests
{
    private readonly MemoryCommerceStore store = new();
    private readonly Mock<IAuthService> auth = new();
    private readonly Guid customerId = Guid.NewGuid();
    private readonly InvoiceRenderer renderer;

    public CommunicationTests()
    {
        auth.Setup(a => a.InitializeAsync()).Returns(Task.CompletedTask);
        auth.SetupGet(a => a.IsAdmin).Returns(true);
        auth.SetupGet(a => a.CurrentUser).Returns(new UserDto { Id = customerId, Role = "Admin" });
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.ContentRootPath).Returns(Path.GetTempPath());
        renderer = new(environment.Object);
    }

    [Fact]
    public void All_mock_accounts_use_the_requested_password()
    {
        DevelopmentBootstrapData.Customers.Should().HaveCount(10).And.OnlyContain(c => c.Password == "123");
    }

    [Fact]
    public void Action_templates_require_links_and_escape_customer_values()
    {
        var template = TemplateService.Default("password-reset");
        var values = new Dictionary<string, string> { ["customer_name"] = "<script>alert(1)</script>", ["store_name"] = "Store", ["action_url"] = "https://example.com/reset?a=1&b=2", ["expiry_minutes"] = "30" };
        var rendered = TemplateService.Render(template, values);
        rendered.HtmlBody.Should().Contain("&lt;script&gt;").And.Contain("a=1&amp;b=2").And.NotContain("<script>");
        template.TextBody = "No action link";
        ((Action)(() => TemplateService.Validate(template))).Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("<a href='javascript:alert(1)'>Click</a>")]
    [InlineData("{{unsupported}}")]
    public void Templates_reject_executable_content_and_unknown_placeholders(string content)
    {
        var template = TemplateService.Default("order-confirmation"); template.HtmlBody = content;
        ((Action)(() => TemplateService.Validate(template))).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Queued_emails_keep_their_published_content_and_are_deduplicated()
    {
        var service = new TemplateService(store, new(auth.Object));
        var template = TemplateService.Default("order-confirmation");
        template.Subject = "Original {{order_reference}}";
        await service.SaveAsync(template, true);
        var values = new Dictionary<string, string> { ["customer_name"] = "Noor", ["store_name"] = "Store", ["order_reference"] = "ABC", ["order_total"] = "EUR 20.00" };
        await store.ExecuteAsync("mail", async tx => { await OutboundEmailService.QueueTemplateAsync(tx, "mail", "noor@example.com", template.Key, values); return true; });
        template.Subject = "Changed {{order_reference}}";
        await service.SaveAsync(template, true);
        await store.ExecuteAsync("mail", async tx => { await OutboundEmailService.QueueTemplateAsync(tx, "mail", "noor@example.com", template.Key, values); return true; });
        (await store.ListAsync<OutboundMessage>()).Should().ContainSingle().Which.Subject.Should().Be("Original ABC");
        (await store.LoadAsync<OutboundMessage>("mail"))!.TemplateVersion.Should().Be(2);
    }

    [Fact]
    public async Task Non_admin_cannot_read_or_publish_templates()
    {
        auth.SetupGet(a => a.IsAdmin).Returns(false);
        var templates = new TemplateService(store, new(auth.Object));
        await ((Func<Task>)(() => templates.ListAsync())).Should().ThrowAsync<UnauthorizedAccessException>();
        await ((Func<Task>)(() => templates.SaveAsync(TemplateService.Default("invoice-email"), true))).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private async Task<Guid> OrderAsync(int lineCount = 1)
    {
        var id = Guid.NewGuid();
        await store.SaveAsync(id.ToString(), new CommerceOrder
        {
            Id = id, CustomerId = customerId, Email = "noor@example.com", PaymentStatus = "Paid",
            Order = new() { Id = id, OrderReference = "TGR-TEST", Total = 24.95m, TaxRate = .09m, ShippingCharge = 4.95m,
                BillingAddress = new() { FirstName = "Zoë", LastName = "de Vries", Street = "Long example street name", HouseNumber = "12", City = "Amsterdam", Country = "NL", PostalCode = "1012 AB" },
                Products = Enumerable.Range(0, lineCount).Select(n => new CookieCommerce.Web.Models.Order.OrderProductDto
                { ProductName = "Chocolate Chunk " + n, ProductVariantName = "5 Cookies Box", Quantity = 2, UnitPrice = 10m / lineCount }).ToList() }
        }); return id;
    }

    [Fact]
    public async Task Concurrent_invoice_issuance_keeps_one_number_and_a_stable_pdf_snapshot()
    {
        var id = await OrderAsync();
        var generator = new InvoiceGenerator(store, renderer);
        var invoices = await Task.WhenAll(generator.EnsureAsync(id), generator.EnsureAsync(id));
        invoices.Select(i => i.Number).Distinct().Should().ContainSingle();
        (await store.ListAsync<InvoiceSequence>()).Single().Next.Should().Be(2);
        var bytes = (await store.LoadAsync<InvoicePdf>(id.ToString()))!.Bytes;
        Encoding.ASCII.GetString(bytes.Take(4).ToArray()).Should().Be("%PDF");
        using (var pdf = PdfReader.Open(new MemoryStream(bytes), PdfDocumentOpenMode.Import)) pdf.PageCount.Should().Be(1);
        var order = (await store.LoadAsync<CommerceOrder>(id.ToString()))!; order.Order.Total = 500;
        await store.SaveAsync(id.ToString(), order);
        await store.SaveAsync("company", new InvoiceSettings { CompanyName = "Changed company" });
        (await generator.EnsureAsync(id)).Order.Total.Should().Be(24.95m);
        (await store.LoadAsync<InvoicePdf>(id.ToString()))!.Bytes.Should().Equal(bytes);
        invoices[0].TaxAmount.Should().Be(2.06m);
    }

    [Fact]
    public async Task Long_invoices_paginate_and_embed_unicode_fonts()
    {
        var id = await OrderAsync(100);
        await new InvoiceGenerator(store, renderer).EnsureAsync(id);
        using var pdf = PdfReader.Open(new MemoryStream((await store.LoadAsync<InvoicePdf>(id.ToString()))!.Bytes), PdfDocumentOpenMode.Import);
        pdf.PageCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Development_invoices_do_not_consume_real_invoice_numbers()
    {
        var demoId = await OrderAsync();
        var demo = (await store.LoadAsync<CommerceOrder>(demoId.ToString()))!; demo.IsDevelopmentSeed = true;
        await store.SaveAsync(demoId.ToString(), demo);
        var generator = new InvoiceGenerator(store, renderer);
        (await generator.EnsureAsync(demoId)).Number.Should().StartWith("DEMO-INV-").And.EndWith("000001");
        (await generator.EnsureAsync(await OrderAsync())).Number.Should().StartWith("INV-").And.EndWith("000001");
    }

    [Fact]
    public async Task A_historical_order_without_tax_information_is_not_given_an_invented_rate()
    {
        var id = Guid.NewGuid();
        await store.SaveHistoricalJsonAsync<CommerceOrder>(id.ToString(), $$$"""{"Id":"{{{id}}}","PaymentStatus":"Paid","Order":{"Total":24.95}}""");
        await ((Func<Task>)(() => new InvoiceGenerator(store, renderer).EnsureAsync(id))).Should().ThrowAsync<InvalidOperationException>().WithMessage("*no saved VAT rate*");
        (await store.ListAsync<InvoiceRecord>()).Should().BeEmpty();
    }

    [Fact]
    public async Task An_invoice_is_not_available_to_another_customer_or_for_an_unpaid_order()
    {
        var id = await OrderAsync();
        var generator = new InvoiceGenerator(store, renderer);
        var service = new InvoiceService(store, generator, new(auth.Object), auth.Object);
        auth.SetupGet(a => a.IsAdmin).Returns(false);
        auth.SetupGet(a => a.CurrentUser).Returns(new UserDto { Id = Guid.NewGuid() });
        await ((Func<Task>)(() => service.DownloadAsync(id))).Should().ThrowAsync<UnauthorizedAccessException>();
        auth.SetupGet(a => a.CurrentUser).Returns(new UserDto { Id = customerId });
        var order = (await store.LoadAsync<CommerceOrder>(id.ToString()))!; order.PaymentStatus = "Pending";
        await store.SaveAsync(id.ToString(), order);
        await ((Func<Task>)(() => service.DownloadAsync(id))).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Smtp_password_is_encrypted_and_environment_settings_are_a_fallback()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Smtp:Host"] = "localhost", ["Smtp:Password"] = "environment-secret" }).Build();
        var reader = new EmailConfigurationReader(store, config, new EphemeralDataProtectionProvider());
        (await reader.ReadAsync()).Password.Should().Be("environment-secret");
        var encrypted = reader.Protect("saved-secret"); encrypted.Should().NotContain("saved-secret");
        await store.SaveAsync("smtp", new StoredEmailConfiguration { Settings = new() { Host = "saved-host" }, ProtectedPassword = encrypted });
        (await reader.ReadAsync()).Password.Should().Be("saved-secret");
        (await reader.ReadAsync()).Settings.Password.Should().BeEmpty();
    }

    [Fact]
    public async Task SendNowAsync_When_smtp_is_not_configured_logs_sanitized_failure_context()
    {
        var secret = "smtp-password-that-must-not-be-logged";
        var recipient = "private-recipient@example.com";
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "",
            ["Smtp:Password"] = secret
        }).Build();
        var logger = new CapturingLogger<OutboundEmailService>();
        var service = new OutboundEmailService(store, new EmailConfigurationReader(store, config, new EphemeralDataProtectionProvider()), logger);

        await ((Func<Task>)(() => service.SendNowAsync(new OutboundMessage
        {
            To = recipient,
            TemplateKey = "backoffice-test",
            Subject = "Test",
            TextBody = "Body",
            ExpiresAt = DateTimeOffset.MaxValue
        }))).Should().ThrowAsync<InvalidOperationException>();

        logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Error);
        logger.Entries.Single().Message.Should().Contain("deliveryKind=backoffice-test")
            .And.Contain("failureReason=invalid-operation")
            .And.NotContain(secret)
            .And.NotContain(recipient);
    }

    [Fact]
    public async Task OperationalHttpLoggingHandler_When_dependency_returns_an_error_logs_safe_context()
    {
        var logger = new CapturingLogger<OperationalHttpLoggingHandler>();
        using var handler = new OperationalHttpLoggingHandler(logger)
        {
            InnerHandler = new RespondingHttpMessageHandler(HttpStatusCode.BadGateway)
        };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://payments.example.test/private?token=must-not-be-logged");

        logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Warning);
        logger.Entries.Single().Message.Should().Contain("dependency=payments.example.test")
            .And.Contain("statusCode=502")
            .And.NotContain("private")
            .And.NotContain("must-not-be-logged");
    }

    private sealed class RespondingHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode));
    }
}
