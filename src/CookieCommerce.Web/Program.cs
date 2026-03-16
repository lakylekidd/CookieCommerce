using CookieCommerce.Web;
using CookieCommerce.Web.Components;
using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Products;
using Microsoft.AspNetCore.Components.Authorization;
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
builder.Services.AddScoped<AuthenticationStateProvider, FakeAuthStateProvider>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddSingleton<ICartService, MockCartService>();
builder.Services.AddScoped<IAuthService, FakeAuthService>();
builder.Services.AddSingleton<IContentService, FakeContentService>();
builder.Services.AddSingleton<ICheckoutService, FakeCheckoutService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IBackofficeService, FakeBackofficeService>();
builder.Services.AddScoped<IImageUploadService, FakeImageUploadService>();
builder.Services.AddSingleton<IAccountService, FakeAccountService>();

builder.Services.AddAuthentication(options =>
{
    // If you are using OpenIddict/Identity, these usually default to 
    // IdentityConstants.ApplicationScheme
    //options.DefaultScheme = "Identity.Application";
    //options.DefaultSignInScheme = "Identity.Application";
    options.DefaultScheme = "FakeCookies";
    options.DefaultChallengeScheme = "FakeCookies";
})
//.AddCookie(); // Or .AddIdentityCookies() if using ASP.NET Core Identity
.AddCookie("FakeCookies", options =>
{
    // Tell the app where to send people if they aren't authorized
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
