using CookieCommerce.Web.Models.Customers;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Services.StaticData;

namespace CookieCommerce.Web.Services;

public class FakeCustomerService : ICustomerService
{
    public Task<CustomerDto> CreateCustomerAsync(string firstName, string lastName, string email, bool sendWelcomeEmail)
    {
        var customer = new CustomerDto { Id = Guid.CreateVersion7(), FirstName = firstName, LastName = lastName, Email = email, MemberSince = DateTime.UtcNow };
        MockCustomerData.Customers.Add(customer);
        return Task.FromResult(customer);
    }

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
