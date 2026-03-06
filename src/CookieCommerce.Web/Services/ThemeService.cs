using Microsoft.JSInterop;

namespace CookieCommerce.Web.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime js) => _js = js;

    // New helper to check state
    public async Task<bool> IsDarkModeAsync()
    {
        return await _js.InvokeAsync<bool>("eval", "document.documentElement.classList.contains('dark')");
    }

    public async Task ToggleThemeAsync()
    {
        var isDark = await IsDarkModeAsync();
        var newTheme = isDark ? "light" : "dark";

        await _js.InvokeVoidAsync("localStorage.setItem", "theme", newTheme);
        await _js.InvokeVoidAsync("eval", $"document.documentElement.classList.toggle('dark', {(!isDark).ToString().ToLower()})");

        OnThemeChanged?.Invoke();
    }
}
