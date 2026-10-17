using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.CustomerDomain.Api.Models.Customers;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

/// <summary>
/// Integration contract tests for CustomerProfile API endpoints (US1).
/// Uses TestWebApplicationFactory which spins up the CustomerDomain API with Postgres via Testcontainers.
/// </summary>
[Collection("Integration")]
public class CustomerProfileApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CustomerProfileApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GET_Me_WithoutCustomerIdHeader_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/customer/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Me_WithNonExistentCustomerId_Returns404()
    {
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("X-Customer-Id", nonExistentId.ToString());

        var response = await client.GetAsync("/api/customer/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_MeProfile_WithoutCustomerIdHeader_Returns401()
    {
        var client = _factory.CreateClient();
        var body = new { firstName = "Jamie", lastName = "Baker", email = "jamie@example.com" };
        var response = await client.PatchAsJsonAsync("/api/customer/me/profile", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_EmailVerification_WithoutCustomerIdHeader_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/customer/me/email-verification", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Me_CreateAndFetch_ReturnsCorrectProfile()
    {
        // Create a customer first via POST /api/customer
        var client = _factory.CreateClient();
        var createBody = new
        {
            email = $"profile-test-{Guid.NewGuid():N}@example.com",
            firstName = "Profile",
            lastName = "Tester",
            gender = 1,  // Male = 1
            dateOfBirth = (DateOnly?)null
        };

        var createResponse = await client.PostAsJsonAsync("/api/customer", createBody);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var customerId = await createResponse.Content.ReadFromJsonAsync<Guid>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Now fetch profile via /me with customer ID header
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());
        var meResponse = await client.GetAsync("/api/customer/me");

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await meResponse.Content.ReadFromJsonAsync<CustomerProfileResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        profile.Should().NotBeNull();
        profile!.Id.Should().Be(customerId);
        profile.FirstName.Should().Be("Profile");
        profile.LastName.Should().Be("Tester");
        profile.Email.Should().Contain("@example.com");
        profile.PendingEmail.Should().BeNull();
        profile.EmailVerificationStatus.Should().Be("Verified");
    }

    [Fact]
    public async Task PATCH_MeProfile_UpdatesNonEmailFields_ReturnsUpdatedProfile()
    {
        var client = _factory.CreateClient();
        var email = $"patch-test-{Guid.NewGuid():N}@example.com";

        // Create
        var createBody = new { email, firstName = "Old", lastName = "Name", gender = 1 };
        var createResponse = await client.PostAsJsonAsync("/api/customer", createBody);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var customerId = await createResponse.Content.ReadFromJsonAsync<Guid>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Update name only (same email)
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());
        var patchBody = new
        {
            firstName = "NewFirst",
            lastName = "NewLast",
            email,  // unchanged
            gender = 2  // Female = 2
        };
        var patchResponse = await client.PatchAsJsonAsync("/api/customer/me/profile", patchBody);
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await patchResponse.Content.ReadFromJsonAsync<CustomerProfileResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        updated!.FirstName.Should().Be("NewFirst");
        updated.LastName.Should().Be("NewLast");
        updated.Email.Should().Be(email);
        updated.PendingEmail.Should().BeNull();
        updated.EmailVerificationStatus.Should().Be("Verified");
    }
}
