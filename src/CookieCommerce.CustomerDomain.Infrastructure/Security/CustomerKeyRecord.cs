namespace CookieCommerce.CustomerDomain.Infrastructure.Security;

public class CustomerKeyRecord
{
    public Guid Id { get; set; }
    public byte[] Key { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
