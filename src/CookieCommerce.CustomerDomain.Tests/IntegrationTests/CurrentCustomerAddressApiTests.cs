using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.CustomerDomain.Api.Models.Customers;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

[Collection("Integration")]
public class CurrentCustomerAddressApiTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly TestWebApplicationFactory _factory;

    public CurrentCustomerAddressApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GET_CurrentAddresses_Returns401_GivenNoCurrentCustomer()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/customer/me/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_CurrentAddresses_ReturnsEmptyArray_GivenCustomerHasNoAddresses()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        var response = await client.GetAsync("/api/customer/me/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var addresses = await response.Content.ReadFromJsonAsync<List<CustomerAddressResponse>>(JsonOptions);
        addresses.Should().NotBeNull();
        addresses.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_CurrentDefaultAddress_Returns204_GivenCustomerHasNoAddresses()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        var response = await client.GetAsync("/api/customer/me/addresses/default");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GET_CurrentAddresses_ReturnsOnlyCurrentCustomersAddresses_GivenTwoCustomers()
    {
        var customerClient = _factory.CreateClient();
        var otherClient = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(customerClient);
        var otherId = await CreateCustomerAsync(otherClient);

        await AddAddressAsync(customerClient, customerId, "Home");
        await AddAddressAsync(otherClient, otherId, "Other");
        customerClient.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        var response = await customerClient.GetAsync("/api/customer/me/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var addresses = await response.Content.ReadFromJsonAsync<List<CustomerAddressResponse>>(JsonOptions);
        addresses.Should().ContainSingle();
        addresses!.Single().Label.Should().Be("Home");
    }

    private static async Task<Guid> CreateCustomerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/customer", new
        {
            email = $"address-api-{Guid.NewGuid():N}@example.com",
            firstName = "Jamie",
            lastName = "Baker",
            gender = 1
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
    }

    private static async Task AddAddressAsync(HttpClient client, Guid customerId, string label)
    {
        var response = await client.PostAsJsonAsync($"/api/customer/{customerId}/addresses", new
        {
            icon = "home",
            label,
            firstName = "Jamie",
            lastName = "Baker",
            street = "Cookie Lane",
            houseNumber = "12",
            houseNumberAddition = (string?)null,
            city = "Baker City",
            postalCode = "1234 AB",
            province = "North Brabant",
            country = "Netherlands",
            phone = "+31612345678",
            isDefault = true
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
