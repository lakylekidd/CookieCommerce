using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.CustomerDomain.Api.Models.Customers;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

[Collection("Integration")]
public class AddCurrentCustomerAddressApiTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly TestWebApplicationFactory _factory;

    public AddCurrentCustomerAddressApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task POST_CurrentAddress_Returns201_GivenValidAddress()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        var response = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Home"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var address = await response.Content.ReadFromJsonAsync<CustomerAddressResponse>(JsonOptions);
        address.Should().NotBeNull();
        address!.Id.Should().NotBeEmpty();
        address.Label.Should().Be("Home");
        address.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task POST_CurrentAddress_DoesNotDuplicateExistingAddress_GivenAddingSecondAddress()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        (await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Home"))).StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Work"));

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var getResponse = await client.GetAsync("/api/customer/me/addresses");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var addresses = await getResponse.Content.ReadFromJsonAsync<List<CustomerAddressResponse>>(JsonOptions);
        addresses.Should().NotBeNull();
        addresses!.Select(x => x.Label).Should().Equal("Home", "Work");
        addresses.Select(x => x.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task PUT_CurrentAddressDefault_MakesRequestedAddressPrimary_GivenExistingAddress()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());
        var homeResponse = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Home", isDefault: true));
        var workResponse = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Work", isDefault: false));
        homeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        workResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var work = await workResponse.Content.ReadFromJsonAsync<CustomerAddressResponse>(JsonOptions);

        var response = await client.PutAsync($"/api/customer/me/addresses/{work!.Id}/default", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await client.GetAsync("/api/customer/me/addresses");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var addresses = await getResponse.Content.ReadFromJsonAsync<List<CustomerAddressResponse>>(JsonOptions);
        addresses.Should().NotBeNull();
        addresses!.Single(x => x.Label == "Home").IsDefault.Should().BeFalse();
        addresses.Single(x => x.Label == "Work").IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task POST_CurrentAddress_Returns422_GivenMissingRequiredFields()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());

        var response = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("label");
    }

    [Fact]
    public async Task POST_CurrentAddress_Returns409_GivenDuplicateLabel()
    {
        var client = _factory.CreateClient();
        var customerId = await CreateCustomerAsync(client);
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());
        (await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Home"))).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("home"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_CurrentAddress_Returns401_GivenNoCurrentCustomer()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Home"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_CurrentAddress_Returns404_GivenCustomerDoesNotExist()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Customer-Id", Guid.CreateVersion7().ToString());

        var response = await client.PostAsJsonAsync("/api/customer/me/addresses", BuildRequest("Home"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static object BuildRequest(string label, bool isDefault = true) => new
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
        isDefault
    };

    private static async Task<Guid> CreateCustomerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/customer", new
        {
            email = $"address-add-api-{Guid.NewGuid():N}@example.com",
            firstName = "Jamie",
            lastName = "Baker",
            gender = 1
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
    }
}
