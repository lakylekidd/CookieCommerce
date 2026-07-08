using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.CustomerDomain.Api.Models.Customers;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

[Collection("Integration")]
public class DeleteCurrentCustomerAddressApiTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly TestWebApplicationFactory _factory;

    public DeleteCurrentCustomerAddressApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DELETE_CurrentAddress_Returns204_GivenOwnedActiveAddress()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());
        var address = await AddAddressAsync(client, "Home");

        var response = await client.DeleteAsync($"/api/customer/me/addresses/{address.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await Task.Delay(500);
        var addresses = await client.GetFromJsonAsync<List<CustomerAddressResponse>>("/api/customer/me/addresses", JsonOptions);
        addresses.Should().BeEmpty();
    }

    [Fact]
    public async Task DELETE_CurrentAddress_Returns400_GivenMalformedAddressId()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        var response = await client.DeleteAsync("/api/customer/me/addresses/not-a-guid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_CurrentAddress_Returns404_GivenMissingAddressId()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        var response = await client.DeleteAsync($"/api/customer/me/addresses/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_CurrentAddress_Returns401_GivenNoCurrentCustomer()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/customer/me/addresses/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DELETE_CurrentAddress_Returns404_GivenAddressBelongsToAnotherCustomer()
    {
        var ownerClient = _factory.CreateClient();
        var ownerId = await CreateCustomerAsync(ownerClient);
        ownerClient.DefaultRequestHeaders.Add("X-Customer-Id", ownerId.ToString());
        var address = await AddAddressAsync(ownerClient, "Home");

        var otherClient = _factory.CreateClient();
        var otherId = await CreateCustomerAsync(otherClient);
        otherClient.DefaultRequestHeaders.Add("X-Customer-Id", otherId.ToString());

        var response = await otherClient.DeleteAsync($"/api/customer/me/addresses/{address.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> CreateCustomerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/customer", new
        {
            email = $"address-delete-api-{Guid.NewGuid():N}@example.com",
            firstName = "Jamie",
            lastName = "Baker",
            gender = 1
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
    }

    private static async Task<CustomerAddressResponse> AddAddressAsync(HttpClient client, string label)
    {
        var response = await client.PostAsJsonAsync("/api/customer/me/addresses", new
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
        return (await response.Content.ReadFromJsonAsync<CustomerAddressResponse>(JsonOptions))!;
    }
}
