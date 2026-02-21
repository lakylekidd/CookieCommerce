using System.Reflection.Metadata.Ecma335;

namespace CookieCommerce.Web.Models.Auth;

public class AuthResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private AuthResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static AuthResult Succeeded() => new(true, null);
    public static AuthResult Failed(string errorMessage) => new(false, errorMessage);
}
