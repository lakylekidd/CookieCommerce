using CookieCommerce.CookieMassTransit;
using CookieCommerce.AuthenticationManagement.Api.Middleware;
using CookieCommerce.AuthenticationManagement.Application;
using CookieCommerce.AuthenticationManagement.Infrastructure;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCookieMassTransit(builder.Configuration, x => x.AddAuthenticationManagementConsumers());
builder.Services.AddMartenConfiguration(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpClient("customer-domain-api", client => client.BaseAddress = new Uri("http://customer-api"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<KeycloakOptions>, IHttpClientFactory>((options, keycloak, factory) =>
    {
        options.Authority = keycloak.Value.BaseUrl.TrimEnd('/') + "/realms/" + keycloak.Value.Realm;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.IncludeErrorDetails = builder.Environment.IsDevelopment();
        options.Backchannel = factory.CreateClient("keycloak");
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudience = keycloak.Value.ClientId;
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                var realmRoles = context.Principal.FindFirst("realm_access")?.Value;
                if (realmRoles is not null)
                {
                    using var roles = JsonDocument.Parse(realmRoles);
                    if (roles.RootElement.TryGetProperty("roles", out var values))
                        foreach (var role in values.EnumerateArray())
                        {
                            var name = role.GetString()!;
                            identity.AddClaim(new Claim(ClaimTypes.Role, name.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : name));
                        }
                }
                var clientRoles = context.Principal.FindFirst("resource_access")?.Value;
                if (clientRoles is not null)
                {
                    using var clients = JsonDocument.Parse(clientRoles);
                    if (clients.RootElement.TryGetProperty(keycloak.Value.ClientId, out var client) && client.TryGetProperty("roles", out var values))
                        foreach (var role in values.EnumerateArray())
                            if (string.Equals(role.GetString(), "Admin", StringComparison.OrdinalIgnoreCase)) identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("recovery", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 10, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Add domain exception handling middleware
app.UseDomainExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program() { }
