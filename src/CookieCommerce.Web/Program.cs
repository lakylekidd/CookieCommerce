using CookieCommerce.Web.Components;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using CookieCommerce.Web;
using CookieCommerce.Web.Services.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // In a homelab, it's often easiest to clear known networks so it trusts your proxy
    // options.KnownNetworks.Clear();
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Data Protection Fix (from our previous step)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/root/.aspnet/DataProtection-Keys"))
    .SetApplicationName("CookieCommerce.Web");

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ICookieService, CookieService>();

var app = builder.Build();

// FIRST: Process proxy headers so app knows it's HTTPS
app.UseForwardedHeaders();

app.MapDefaultEndpoints();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.AddSiteMapOptions();
app.AddRobots();

app.Run();
