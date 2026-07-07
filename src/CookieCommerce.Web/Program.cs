using CookieCommerce.Web;
using CookieCommerce.Web.Components;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Products;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // In a homelab, it's often easiest to clear known networks so it trusts your proxy
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Data Protection Fix (from our previous step)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/root/.aspnet/DataProtection-Keys"))
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
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddSingleton<ICartService, MockCartService>();
builder.Services.AddHttpClient("authentication-management-api", client =>
{
    client.BaseAddress = new Uri("http://authentication-management-api");
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
    return new KeycloakAuthService(httpClient, cartService, localStorage);
});
builder.Services.AddSingleton<IContentService, FakeContentService>();
builder.Services.AddSingleton<ICheckoutService, FakeCheckoutService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IBackofficeService, FakeBackofficeService>();
builder.Services.AddScoped<IImageUploadService, FakeImageUploadService>();
builder.Services.AddScoped<IOperationsService, FakeOperationsService>();
builder.Services.AddScoped<IAccountService, FakeAccountService>();

// CustomerDomain API: live profile service (US1)
builder.Services.AddHttpClient("customer-domain-api", client =>
{
    client.BaseAddress = new Uri("http://customer-api");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<CustomerProfileService>();
builder.Services.AddScoped<CustomerProfileState>();
builder.Services.AddScoped<IInventoryService, FakeInventoryService>();
builder.Services.AddScoped<ICustomerService, FakeCustomerService>();
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
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.AddSiteMapOptions();
app.AddRobots();

app.Run();
