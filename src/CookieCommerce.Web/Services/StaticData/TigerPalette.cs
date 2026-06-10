namespace CookieCommerce.Web.Services.StaticData;

public static class TigerPalette
{
    public static readonly string[] Colors =
    [
        "bg-amber-100 dark:bg-amber-900/30 text-amber-600",
        "bg-orange-100 dark:bg-orange-900/30 text-orange-600",
        "bg-cyan-100 dark:bg-cyan-900/30 text-cyan-600",
        "bg-blue-100 dark:bg-blue-900/30 text-blue-600",
        "bg-purple-100 dark:bg-purple-900/30 text-purple-600",
        "bg-violet-100 dark:bg-violet-900/30 text-violet-600"
    ];

    public static string GetColorFromString(string input)
    {
        if (string.IsNullOrEmpty(input)) return Colors[0];
        // Use the hash code of the string to pick a consistent index
        var index = Math.Abs(input.GetHashCode()) % Colors.Length;
        return Colors[index];
    }
}
