using System.Text.RegularExpressions;

namespace CookieCommerce.AuthenticationManagement.Infrastructure.Keycloak;

public static class KeycloakPasswordPolicy
{
    public static string Resolve(bool development, string existing)
    {
        if (development) return "length(3)";
        var length = Regex.Match(existing, @"\blength\((\d+)\)");
        if (length.Success && int.Parse(length.Groups[1].Value) >= 8) return existing;
        if (length.Success) return existing.Replace(length.Value, "length(8)");
        return string.IsNullOrWhiteSpace(existing) ? "length(8)" : existing + " and length(8)";
    }
}
