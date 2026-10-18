using CookieCommerce.Web;
using CookieCommerce.Web.Components;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Products;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Services.Payments;
using Npgsql;
using CookieCommerce.Web.Services.Development;
using CookieCommerce.Web.Services.Communications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // In a homelab, it's often easiest to clear known networks so it trusts your proxy
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Data Protection Fix (from our previous step)
var protectionPath = builder.Configuration["DataProtection:KeyPath"] ?? Path.Combine(builder.Environment.ContentRootPath, ".data-protection");
Directory.CreateDirectory(protectionPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(protectionPath))
    .SetApplicationName("CookieCommerce.Web");

builder.AddServiceDefaults();

builder.Services.AddServerSideBlazor().AddHubOptions(options =>
{
    // Set to 10MB or whatever fits your cookie photos
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<AuthenticationStateProvider, KeycloakAuthStateProvider>();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<ProductApiClient>();
builder.Services.AddScoped<ProductMainImages>();
builder.Services.AddScoped<ICookieService, CatalogService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<ICartService>(sp => sp.GetRequiredService<CartService>());
builder.Services.AddSingleton(sp => NpgsqlDataSource.Create(builder.Configuration.GetConnectionString("Commerce")
    ?? throw new InvalidOperationException("Connection string 'Commerce' is required. Start the site through AppHost or configure PostgreSQL.")));
builder.Services.AddSingleton<CommerceStore>();
builder.Services.AddSingleton<ICommerceStore>(sp => sp.GetRequiredService<CommerceStore>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<CommerceStore>());
builder.Services.AddHostedService<CommerceAutomation>();
builder.Services.AddSingleton<EmailConfigurationReader>();
builder.Services.AddSingleton<OutboundEmailService>();
builder.Services.AddHostedService<OutboundEmailWorker>();
builder.Services.AddScoped<EmailConfigurationService>();
builder.Services.AddScoped<AccountAdministrationService>();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddSingleton<InvoiceRenderer>();
builder.Services.AddSingleton<InvoiceGenerator>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddHostedService<InvoiceWorker>();
builder.Services.AddScoped<DevelopmentCommerceSeeder>();
builder.Services.AddHostedService<DevelopmentCommerceBootstrapService>();
builder.Services.AddScoped<CommerceAccess>();
builder.Services.AddScoped<AccountFeatures>();
builder.Services.AddScoped<StoreSettingsService>();
builder.Services.AddHttpClient("product-domain-api", client =>
    client.BaseAddress = new Uri(builder.Configuration["ServiceEndpoints:ProductApi"] ?? "http://product-api"));
builder.Services.AddHttpClient("payments", client => client.Timeout = TimeSpan.FromSeconds(30));
builder.Services.Configure<PaymentOptions>(builder.Configuration.GetSection("Payments"));
builder.Services.AddSingleton<StripePaymentGateway>();
builder.Services.AddSingleton<MolliePaymentGateway>();
builder.Services.AddSingleton<IPaymentGateway>(sp => sp.GetRequiredService<StripePaymentGateway>());
builder.Services.AddSingleton<IPaymentGateway>(sp => sp.GetRequiredService<MolliePaymentGateway>());
builder.Services.AddSingleton<PaymentProcessor>();
builder.Services.AddHttpClient("authentication-management-api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceEndpoints:AuthenticationManagementApi"] ?? "https+http://authentication-management-api");
    client.Timeout = TimeSpan.FromSeconds(60);
})
.AddStandardResilienceHandler(options =>
{
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
    options.Retry.MaxRetryAttempts = 1;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
});
builder.Services.AddScoped<IAuthService>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("authentication-management-api");
    var cartService = sp.GetRequiredService<ICartService>();
    var localStorage = sp.GetRequiredService<ProtectedLocalStorage>();
    return new KeycloakAuthService(httpClient, cartService, localStorage, sp.GetRequiredService<ILogger<KeycloakAuthService>>());
});
builder.Services.AddSingleton<IContentService, ContentService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<ICheckoutService>(sp => sp.GetRequiredService<CheckoutService>());
builder.Services.AddHttpClient<ILocationService, LocationService>();
builder.Services.AddScoped<BackofficeService>();
builder.Services.AddScoped<IBackofficeService>(sp => sp.GetRequiredService<BackofficeService>());
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IOperationsService>(sp => sp.GetRequiredService<StoreSettingsService>());
builder.Services.AddScoped<IAccountService, CustomerDomainAccountService>();

// CustomerDomain API: live profile and address services (US1)
builder.Services.AddHttpClient("customer-domain-api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceEndpoints:CustomerApi"] ?? "http://customer-api");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<CustomerProfileService>();
builder.Services.AddScoped<CustomerProfileState>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<IInventoryService>(sp => sp.GetRequiredService<InventoryService>());
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.Configure<ListmonkOptions>(builder.Configuration.GetSection("Listmonk"));
builder.Services.AddHttpClient<INewsletterPublisher, ListmonkNewsletterPublisher>(client => client.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddScoped<IContactService, SmtpContactService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/auth/signin";
    options.AccessDeniedPath = "/access-denied";
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

app.UseForwardedHeaders();
app.MapDefaultEndpoints();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseStaticFiles();
app.UseAntiforgery();
app.MapCommerceEndpoints();
app.MapCommunicationEndpoints();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.AddSiteMapOptions();
app.AddRobots();

app.Run();

public partial class Program { }
