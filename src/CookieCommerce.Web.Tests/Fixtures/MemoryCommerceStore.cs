using System.Text.Json;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Tests.Fixtures;

// Deep copies and transaction rollback model the durable store's behavior.
public sealed class MemoryCommerceStore : ICommerceStore
{
    private Dictionary<(Type, string), string> documents = [];
    private readonly SemaphoreSlim gate = new(1);
    public Task<TResult> ExecuteAsync<TResult>(IReadOnlyList<string> lockKeys, Func<ICommerceTransaction, Task<TResult>> action) => ExecuteAsync(string.Join('|', lockKeys), action);
    public Task<T?> LoadAsync<T>(string key) => Task.FromResult(Read<T>(documents, key));
    public Task<IReadOnlyList<T>> ListAsync<T>() => Task.FromResult<IReadOnlyList<T>>(documents.Where(d => d.Key.Item1 == typeof(T)).Select(d => JsonSerializer.Deserialize<T>(d.Value)!).ToList());
    public async Task<TResult> ExecuteAsync<TResult>(string lockKey, Func<ICommerceTransaction, Task<TResult>> action)
    {
        await gate.WaitAsync();
        try
        {
            var working = new Dictionary<(Type, string), string>(documents);
            var result = await action(new Transaction(working));
            documents = working;
            return result;
        }
        finally { gate.Release(); }
    }
    public Task SaveAsync<T>(string key, T value) => ExecuteAsync(key, async tx => { await tx.SaveAsync(key, value); return true; });
    public async Task SaveHistoricalJsonAsync<T>(string key, string json)
    {
        await gate.WaitAsync();
        try { documents[(typeof(T), key)] = json; }
        finally { gate.Release(); }
    }
    private static T? Read<T>(Dictionary<(Type, string), string> data, string key) => data.TryGetValue((typeof(T), key), out var json) ? JsonSerializer.Deserialize<T>(json) : default;
    private sealed class Transaction(Dictionary<(Type, string), string> data) : ICommerceTransaction
    {
        public Task<IReadOnlyList<T>> ListAsync<T>() => Task.FromResult<IReadOnlyList<T>>(data.Where(d => d.Key.Item1 == typeof(T)).Select(d => JsonSerializer.Deserialize<T>(d.Value)!).ToList());
        public Task<T?> LoadAsync<T>(string key) => Task.FromResult(Read<T>(data, key));
        public Task SaveAsync<T>(string key, T value) { data[(typeof(T), key)] = JsonSerializer.Serialize(value); return Task.CompletedTask; }
        public Task DeleteAsync<T>(string key) { data.Remove((typeof(T), key)); return Task.CompletedTask; }
    }
}
