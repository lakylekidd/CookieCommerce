namespace CookieCommerce.Web.Models;

public class ResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]> FieldErrors { get; set; } = new();

    public static ResultDto Ok(string message = "Success") => new() { Success = true, Message = message };
    public static ResultDto Fail(string message) => new() { Success = false, Message = message };
}
