using CookieCommerce.OrderDomain.Application.Customers;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CookieCommerce.OrderDomain.Infrastructure.Customers;

/// <summary>
/// Service for retrieving customer information from the Customer API.
/// Implements graceful failure handling when customer API is unavailable or customer doesn't exist.
/// </summary>
public class CustomerService(HttpClient httpClient, ILogger<CustomerService> logger) : ICustomerService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CustomerService> _logger = logger;

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching customer {CustomerId} from Customer API", customerId);
            
            var response = await _httpClient.GetAsync($"/api/customer/{customerId}", cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Customer {CustomerId} not found in Customer API", customerId);
                return null;
            }

            response.EnsureSuccessStatusCode();
            
            var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken);
            
            _logger.LogInformation("Successfully retrieved customer {CustomerId}", customerId);
            
            return customer;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to retrieve customer {CustomerId} from Customer API", customerId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving customer {CustomerId}", customerId);
            return null;
        }
    }
}
