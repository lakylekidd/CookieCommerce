using CookieCommerce.Web.Models.Customers;
using CookieCommerce.Web.Models.Order;

namespace CookieCommerce.Web.Services;

public interface ICustomerService
{
    Task<ICollection<CustomerDto>> GetCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
    Task<ICollection<OrderListItemDto>> GetCustomerOrdersAsync(Guid customerId);
}
