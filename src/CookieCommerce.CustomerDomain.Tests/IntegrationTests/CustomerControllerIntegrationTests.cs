using System.Net;
using System.Net.Http.Json;
using Alba;
using CookieCommerce.CustomerDomain.Api.Models.Customers;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class CustomerControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CustomerControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateCustomer_ValidRequest_ReturnsCreatedWithId()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateCustomerRequest
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/customer", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Assert
        response.Headers.Location.Should().NotBeNull();
        var location = response.Headers.Location!.ToString();
        location.Should().Contain("/api/Customer/");
    }

    [Fact]
    public async Task CreateCustomer_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateCustomerRequest
        {
            Email = "", // Required field missing
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        var response = await client.PostAsJsonAsync("/api/customer", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomerById_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange - create a customer first
        var client = _factory.CreateClient();
        var createRequest = new CreateCustomerRequest
        {
            Email = "get-test@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            Gender = Gender.Female
        };

        var createResponse = await client.PostAsJsonAsync("/api/customer", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var customerId = location.Split('/').Last();

        // Give projection time to process
        await Task.Delay(500);

        // Act
        var getResponse = await client.GetAsync($"/api/customer/{customerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        var customer = await getResponse.Content.ReadFromJsonAsync<CustomerReadModel>();
        customer.Should().NotBeNull();
        customer!.Email.Should().Be("get-test@example.com");
        customer.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetCustomerById_NonExistentCustomer_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var response = await client.GetAsync($"/api/customer/{nonExistentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_ExistingCustomer_ReturnsNoContent()
    {
        // Arrange - create a customer first
        var client = _factory.CreateClient();
        var createRequest = new CreateCustomerRequest
        {
            Email = "delete-test@example.com",
            FirstName = "Delete",
            LastName = "Test",
            Gender = Gender.Male
        };

        var createResponse = await client.PostAsJsonAsync("/api/customer", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var customerId = location.Split('/').Last();

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/customer/{customerId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddAddress_ValidRequest_ReturnsCreated()
    {
        // Arrange - create a customer first
        var client = _factory.CreateClient();
        var createRequest = new CreateCustomerRequest
        {
            Email = "address-test@example.com",
            FirstName = "Address",
            LastName = "Test",
            Gender = Gender.Female
        };

        var createResponse = await client.PostAsJsonAsync("/api/customer", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var customerId = location.Split('/').Last();

        var addressRequest = new AddCustomerAddressRequest
        {
            Label = "Home",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            IsPrimary = true
        };

        // Act & Assert
        var addResponse = await client.PostAsJsonAsync($"/api/customer/{customerId}/addresses/", addressRequest);
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Note: Address projection not yet implemented in CustomerReadModel
        // Once projection handles CustomerAddressAdded, verify addresses here
    }

    [Fact]
    public async Task AddAddress_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var client = _factory.CreateClient();
        var addressRequest = new AddCustomerAddressRequest
        {
            Label = "", // Required field missing
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA"
        };

        // Act & Assert
        var response = await client.PostAsJsonAsync($"/api/customer/{customerId}/addresses/", addressRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddAddress_DuplicateLabel_ReturnsError()
    {
        // Arrange - create a customer and add an address
        var client = _factory.CreateClient();
        var createRequest = new CreateCustomerRequest
        {
            Email = "duplicate-address-test@example.com",
            FirstName = "Duplicate",
            LastName = "Test",
            Gender = Gender.Male
        };

        var createResponse = await client.PostAsJsonAsync("/api/customer", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var customerId = location.Split('/').Last();

        var addressRequest = new AddCustomerAddressRequest
        {
            Label = "Home",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA"
        };

        // Add first address
        var addResponse = await client.PostAsJsonAsync($"/api/customer/{customerId}/addresses/", addressRequest);
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await Task.Delay(500);

        // Act - try to add duplicate
        var duplicateResponse = await client.PostAsJsonAsync($"/api/customer/{customerId}/addresses/", addressRequest);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // The aggregate should throw an exception for duplicate label
        // You may need to adjust expected status code based on error handling
    }

    [Fact]
    public async Task RemoveAddress_ExistingAddress_ReturnsNoContent()
    {
        // Arrange - create customer and add address
        var client = _factory.CreateClient();
        var createRequest = new CreateCustomerRequest
        {
            Email = "remove-address-test@example.com",
            FirstName = "Remove",
            LastName = "Test",
            Gender = Gender.Female
        };

        var createResponse = await client.PostAsJsonAsync("/api/customer", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var customerId = location.Split('/').Last();

        var addressRequest = new AddCustomerAddressRequest
        {
            Label = "Work",
            Street = "456 Office Blvd",
            City = "Chicago",
            State = "IL",
            PostalCode = "60601",
            Country = "USA"
        };

        var addResponse = await client.PostAsJsonAsync($"/api/customer/{customerId}/addresses/", addressRequest);
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await Task.Delay(500);

        // Act & Assert
        var removeResponse = await client.DeleteAsync($"/api/customer/{customerId}/addresses/Work");
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Note: Address projection not yet implemented in CustomerReadModel
        // Once projection handles CustomerAddressRemoved, verify addresses removed here
    }

    [Fact]
    public async Task RemoveAddress_NonExistentAddress_ReturnsError()
    {
        // Arrange - create customer without addresses
        var client = _factory.CreateClient();
        var createRequest = new CreateCustomerRequest
        {
            Email = "remove-nonexistent-test@example.com",
            FirstName = "Test",
            LastName = "User",
            Gender = Gender.Male
        };

        var createResponse = await client.PostAsJsonAsync("/api/customer", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location!.ToString();
        var customerId = location.Split('/').Last();

        await Task.Delay(500);

        // Act & Assert - try to remove non-existent address
        var removeResponse = await client.DeleteAsync($"/api/customer/{customerId}/addresses/NonExistent");
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // The aggregate should throw an exception for non-existent label
        // You may need to adjust expected status code based on error handling
    }
}
