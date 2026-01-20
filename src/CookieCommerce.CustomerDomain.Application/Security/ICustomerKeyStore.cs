namespace CookieCommerce.CustomerDomain.Application.Security;

public interface ICustomerKeyStore
{
    Task<byte[]> CreateKeyForCustomerAsync(Guid customerId, CancellationToken ct);
    Task<byte[]?> GetKeyAsync(Guid customerId, CancellationToken ct);
    Task DeleteKeyAsync(Guid customerId, CancellationToken ct);
}
