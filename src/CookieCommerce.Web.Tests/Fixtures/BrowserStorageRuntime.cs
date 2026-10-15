using Microsoft.JSInterop;

namespace CookieCommerce.Web.Tests.Fixtures;

public sealed class BrowserStorageRuntime : IJSRuntime
{
    private readonly Dictionary<string, string> values = [];
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        var key = (string)args![0]!;
        object? result = null;
        if (identifier == "localStorage.getItem") result = values.GetValueOrDefault(key);
        else if (identifier == "localStorage.setItem") values[key] = (string)args[1]!;
        else if (identifier == "localStorage.removeItem") values.Remove(key);
        else throw new InvalidOperationException(identifier);
        return ValueTask.FromResult(result is null ? default! : (TValue)result);
    }
}
