using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Npgsql;

namespace CookieCommerce.Web.Services.Commerce;

public interface ICommerceStore
{
    Task<T?> LoadAsync<T>(string key);
    Task<IReadOnlyList<T>> ListAsync<T>();
    Task<TResult> ExecuteAsync<TResult>(string lockKey, Func<ICommerceTransaction, Task<TResult>> action);
    Task<TResult> ExecuteAsync<TResult>(IReadOnlyList<string> lockKeys, Func<ICommerceTransaction, Task<TResult>> action);
}

public interface ICommerceTransaction
{
    Task<IReadOnlyList<T>> ListAsync<T>();
    Task<T?> LoadAsync<T>(string key);
    Task SaveAsync<T>(string key, T value);
    Task DeleteAsync<T>(string key);
}

/// <summary>Operational commerce storage until the remaining bounded contexts expose APIs.
/// Each write locks its business key across all web instances and commits atomically.
/// Payloads containing customer data are encrypted with the shared data protection key ring.</summary>
public sealed class CommerceStore(NpgsqlDataSource source, IDataProtectionProvider protection) : ICommerceStore, IHostedService
{
    private readonly IDataProtector _protector = protection.CreateProtector("CookieCommerce.Commerce.v1");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var command = source.CreateCommand("""
            CREATE TABLE IF NOT EXISTS commerce_documents (
                kind text NOT NULL, key text NOT NULL, payload text NOT NULL,
                PRIMARY KEY (kind, key));
            """);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task<T?> LoadAsync<T>(string key)
    {
        await using var connection = await source.OpenConnectionAsync();
        return await new Transaction(connection, null, _protector).LoadAsync<T>(key);
    }

    public async Task<IReadOnlyList<T>> ListAsync<T>()
    {
        await using var command = source.CreateCommand("SELECT payload FROM commerce_documents WHERE kind = $1");
        command.Parameters.AddWithValue(typeof(T).Name);
        await using var reader = await command.ExecuteReaderAsync();
        var results = new List<T>();
        while (await reader.ReadAsync())
            results.Add(JsonSerializer.Deserialize<T>(_protector.Unprotect(reader.GetString(0)), JsonOptions)!);
        return results;
    }

    public Task<TResult> ExecuteAsync<TResult>(string lockKey, Func<ICommerceTransaction, Task<TResult>> action) => ExecuteAsync([lockKey], action);

    public async Task<TResult> ExecuteAsync<TResult>(IReadOnlyList<string> lockKeys, Func<ICommerceTransaction, Task<TResult>> action)
    {
        await using var connection = await source.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        foreach (var key in lockKeys.Distinct().Order(StringComparer.Ordinal))
        {
            await using var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended($1, 0))", connection, transaction);
            command.Parameters.AddWithValue(key);
            await command.ExecuteNonQueryAsync();
        }
        var result = await action(new Transaction(connection, transaction, _protector));
        await transaction.CommitAsync();
        return result;
    }

    private sealed class Transaction(NpgsqlConnection connection, NpgsqlTransaction? transaction, IDataProtector protector) : ICommerceTransaction
    {
        public async Task<IReadOnlyList<T>> ListAsync<T>()
        {
            await using var command = new NpgsqlCommand("SELECT payload FROM commerce_documents WHERE kind=$1", connection, transaction);
            command.Parameters.AddWithValue(typeof(T).Name);
            await using var reader = await command.ExecuteReaderAsync();
            var results = new List<T>();
            while (await reader.ReadAsync()) results.Add(JsonSerializer.Deserialize<T>(protector.Unprotect(reader.GetString(0)), JsonOptions)!);
            return results;
        }
        public async Task<T?> LoadAsync<T>(string key)
        {
            await using var command = new NpgsqlCommand("SELECT payload FROM commerce_documents WHERE kind=$1 AND key=$2", connection, transaction);
            command.Parameters.AddWithValue(typeof(T).Name);
            command.Parameters.AddWithValue(key);
            var payload = await command.ExecuteScalarAsync() as string;
            return payload is null ? default : JsonSerializer.Deserialize<T>(protector.Unprotect(payload), JsonOptions);
        }

        public async Task SaveAsync<T>(string key, T value)
        {
            await using var command = new NpgsqlCommand("""
                INSERT INTO commerce_documents(kind,key,payload) VALUES ($1,$2,$3)
                ON CONFLICT (kind,key) DO UPDATE SET payload=EXCLUDED.payload
                """, connection, transaction);
            command.Parameters.AddWithValue(typeof(T).Name);
            command.Parameters.AddWithValue(key);
            command.Parameters.AddWithValue(protector.Protect(JsonSerializer.Serialize(value, JsonOptions)));
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync<T>(string key)
        {
            await using var command = new NpgsqlCommand("DELETE FROM commerce_documents WHERE kind=$1 AND key=$2", connection, transaction);
            command.Parameters.AddWithValue(typeof(T).Name);
            command.Parameters.AddWithValue(key);
            await command.ExecuteNonQueryAsync();
        }
    }
}
