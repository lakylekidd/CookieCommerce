using System.Reflection;
using System.Text;
using System.Text.Json;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using FluentAssertions;
using CookieCommerce.Web.Tests.Fixtures;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace CookieCommerce.Web.Tests.Services;

public class KeycloakAuthServiceTests
{
    [Fact]
    public async Task Concurrent_initialization_refreshes_expired_credentials_once()
    {
        var browser = new ProtectedLocalStorage(new BrowserStorageRuntime(), new EphemeralDataProtectionProvider());
        var userId = Guid.CreateVersion7();
        await browser.SetAsync("auth_access_token", CreateJwt(new { sub = userId, exp = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds() }));
        await browser.SetAsync("auth_refresh_token", "refresh");
        var fresh = CreateJwt(new { sub = userId, email = "jamie@example.com", exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() });
        var handler = new RefreshHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { accessToken = fresh, refreshToken = "rotated", expiresIn = 3600 }) });
        var cart = new Mock<ICartService>();
        cart.Setup(c => c.ConvertToUserCartAsync(userId)).Returns(Task.CompletedTask);
        var service = new KeycloakAuthService(new HttpClient(handler) { BaseAddress = new Uri("http://auth/") }, cart.Object, browser);
        await Task.WhenAll(service.InitializeAsync(), service.InitializeAsync());
        handler.Calls.Should().Be(1);
        service.CurrentUser!.Id.Should().Be(userId);
        cart.Verify(c => c.ConvertToUserCartAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Rejected_refresh_clears_credentials_without_restarting_initialization()
    {
        var browser = new ProtectedLocalStorage(new BrowserStorageRuntime(), new EphemeralDataProtectionProvider());
        await browser.SetAsync("auth_access_token", CreateJwt(new { sub = Guid.CreateVersion7(), exp = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds() }));
        await browser.SetAsync("auth_refresh_token", "invalid");
        var handler = new RefreshHandler(new(HttpStatusCode.Unauthorized));
        var service = new KeycloakAuthService(new HttpClient(handler) { BaseAddress = new Uri("http://auth/") }, Mock.Of<ICartService>(), browser);
        service.OnChange += () => _ = service.InitializeAsync();
        await service.InitializeAsync();
        await service.InitializeAsync();
        handler.Calls.Should().Be(1);
        service.CurrentUser.Should().BeNull();
        (await browser.GetAsync<string>("auth_access_token")).Success.Should().BeFalse();
    }

    private sealed class RefreshHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            await Task.Delay(20, cancellationToken);
            return response;
        }
    }
    [Fact]
    public void ParseJwtToUserDto_recognizes_admin_realm_role_from_base64url_jwt()
    {
        var token = CreateJwt(new
        {
            sub = Guid.Parse("11111111-1111-1111-1111-111111111111").ToString(),
            email = "admin@tiger.com",
            given_name = "Tiger",
            family_name = "Baker",
            // Forces '-' in the base64url payload so Convert.FromBase64String-only decoding would fail.
            nonce = "K~<IJ?5Wx7\\.VPDG>Z_l<Ga_2F1r9KC'}~>jJN!^#-k_YifuQ8",
            realm_access = new { roles = new[] { "offline_access", "admin" } }
        });

        var user = ParseJwtToUserDto(token);

        user.Should().NotBeNull();
        user!.Email.Should().Be("admin@tiger.com");
        user.Role.Should().Be("Admin");
    }

    [Fact]
    public void ParseJwtToUserDto_recognizes_admin_client_role()
    {
        var token = CreateJwt(new
        {
            sub = Guid.Parse("22222222-2222-2222-2222-222222222222").ToString(),
            email = "admin@tiger.com",
            resource_access = new Dictionary<string, object>
            {
                ["cookiecommerce-backend"] = new { roles = new[] { "admin" } }
            }
        });

        var user = ParseJwtToUserDto(token);

        user.Should().NotBeNull();
        user!.Role.Should().Be("Admin");
    }

    private static UserDto? ParseJwtToUserDto(string token)
    {
        var method = typeof(KeycloakAuthService).GetMethod("ParseJwtToUserDto", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();
        return (UserDto?)method!.Invoke(null, [token]);
    }

    private static string CreateJwt(object payload)
    {
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "none", typ = "JWT" }));
        var body = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signature = Base64UrlEncode(Encoding.UTF8.GetBytes("signature"));
        return $"{header}.{body}.{signature}";
    }

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
