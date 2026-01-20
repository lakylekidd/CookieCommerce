using CookieCommerce.CustomerDomain.Application.Security;
using Marten;
using System.Security.Cryptography;

namespace CookieCommerce.CustomerDomain.Infrastructure.Security;

public class CustomerKeyStore(IDocumentSession session) : ICustomerKeyStore
{
    public Task<byte[]> CreateKeyForCustomerAsync(Guid customerId, CancellationToken ct)
    {
        var key = RandomNumberGenerator.GetBytes(32);

        var record = new CustomerKeyRecord
        {
            Id = customerId,
            Key = key,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(record);
        return session.SaveChangesAsync(ct).ContinueWith(_ => key, ct);
    }

    public Task DeleteKeyAsync(Guid customerId, CancellationToken ct)
    {
        session.Delete<CustomerKeyRecord>(customerId);
        return session.SaveChangesAsync(ct);
    }

    public Task<byte[]?> GetKeyAsync(Guid customerId, CancellationToken ct)
    {
        var record = session.LoadAsync<CustomerKeyRecord>(customerId, ct);
        return record.ContinueWith(r => r.Result?.Key, ct);
    }
}
