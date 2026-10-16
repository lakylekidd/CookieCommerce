using CookieCommerce.Web.Models.Order;

namespace CookieCommerce.Web.Services.Communications;

public sealed class InvoiceRecord
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public DateTimeOffset IssuedAt { get; set; }
    public Guid? CustomerId { get; set; }
    public string Email { get; set; } = "";
    public OrderDto Order { get; set; } = new();
    public InvoiceSettings Company { get; set; } = new();
    public MessageTemplate Template { get; set; } = new();
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string State { get; set; } = "Pending";
    public string ContentHash { get; set; } = "";
    public bool IsDevelopmentSeed { get; set; }
    public byte[] LogoBytes { get; set; } = [];
}

public sealed class InvoicePdf
{
    public Guid Id { get; set; }
    public byte[] Bytes { get; set; } = [];
}

public sealed class InvoiceSequence { public int Next { get; set; } = 1; }
