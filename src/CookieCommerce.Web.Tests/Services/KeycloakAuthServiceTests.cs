using System.Reflection;
using System.Text;
using System.Text.Json;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using FluentAssertions;

namespace CookieCommerce.Web.Tests.Services;

public class KeycloakAuthServiceTests
{
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
