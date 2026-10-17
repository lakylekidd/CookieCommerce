using CookieCommerce.Web.Models.Customers;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeCustomerService : ICustomerService
{
    public async Task<ICollection<CustomerDto>> GetCustomersAsync()
    {
        await Task.Delay(600);
        return MockCustomerData.Customers;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        await Task.Delay(400);
        return MockCustomerData.Customers.FirstOrDefault(c => c.Id == id);
    }

    public async Task<ICollection<OrderListItemDto>> GetCustomerOrdersAsync(Guid customerId)
    {
        await Task.Delay(500);
        var customer = MockCustomerData.Customers.FirstOrDefault(c => c.Id == customerId);
        if (customer is null) return [];
        return MockCustomerData.GenerateOrdersForCustomer(customer);
    }
}
