namespace CookieCommerce.OrderDomain.Domain.ValueObjects;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Paid = 2,
    Cancelled = 3,
    Rejected = 4
}
