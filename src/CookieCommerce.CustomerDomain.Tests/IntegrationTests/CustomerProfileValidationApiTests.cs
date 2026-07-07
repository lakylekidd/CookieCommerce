using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

/// <summary>
/// US2 – API contract tests: invalid profile update requests must return field-level
/// validation problem responses (422 with errors dict) and no saved state changes.
/// Uses TestWebApplicationFactory (Testcontainers PostgreSQL).
/// </summary>
[Collection("Integration")]
public class CustomerProfileValidationApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public CustomerProfileValidationApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<(HttpClient client, Guid customerId)> CreateCustomerAsync(string email)
    {
        var client = _factory.CreateClient();
        var body = new
        {
            email,
            firstName = "Validation",
            lastName = "Tester",
            gender = 1,  // Male
            dateOfBirth = (DateOnly?)null
        };
        var response = await client.PostAsJsonAsync("/api/customer", body, JsonOpts);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var customerId = await response.Content.ReadFromJsonAsync<Guid>(JsonOpts);
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Customer-Id", customerId.ToString());
        return (client, customerId);
    }

    // ─── Empty first name → 422 with firstName field error ───────────────────

    [Fact]
    public async Task PATCH_MeProfile_EmptyFirstName_Returns422WithFirstNameError()
    {
        var email = $"val-fn-{Guid.NewGuid():N}@example.com";
        var (client, _) = await CreateCustomerAsync(email);

        var body = new
        {
            firstName = "",
            lastName = "Tester",
            email,
            gender = 1
        };
        var response = await client.PatchAsJsonAsync("/api/customer/me/profile", body, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var errors = await ReadErrorsAsync(response);
        errors.Should().ContainKey("firstName");
    }

    // ─── Empty last name → 422 with lastName field error ─────────────────────

    [Fact]
    public async Task PATCH_MeProfile_EmptyLastName_Returns422WithLastNameError()
    {
        var email = $"val-ln-{Guid.NewGuid():N}@example.com";
        var (client, _) = await CreateCustomerAsync(email);

        var body = new
        {
            firstName = "Validation",
            lastName = "",
            email,
            gender = 1
        };
        var response = await client.PatchAsJsonAsync("/api/customer/me/profile", body, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var errors = await ReadErrorsAsync(response);
        errors.Should().ContainKey("lastName");
    }

    // ─── Invalid email format → 422 with email field error ───────────────────

    [Fact]
    public async Task PATCH_MeProfile_InvalidEmailFormat_Returns422WithEmailError()
    {
        var email = $"val-email-{Guid.NewGuid():N}@example.com";
        var (client, _) = await CreateCustomerAsync(email);

        var body = new
        {
            firstName = "Validation",
            lastName = "Tester",
            email = "not-a-valid-email",
            gender = 1
        };
        var response = await client.PatchAsJsonAsync("/api/customer/me/profile", body, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var errors = await ReadErrorsAsync(response);
        errors.Should().ContainKey("email");
    }

    // ─── Future date of birth → 422 with dateOfBirth field error ─────────────

    [Fact]
    public async Task PATCH_MeProfile_FutureDateOfBirth_Returns422WithDateOfBirthError()
    {
        var email = $"val-dob-{Guid.NewGuid():N}@example.com";
        var (client, _) = await CreateCustomerAsync(email);

        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var body = new
        {
            firstName = "Validation",
            lastName = "Tester",
            email,
            gender = 1,
            dateOfBirth = futureDate.ToString("yyyy-MM-dd")
        };
        var response = await client.PatchAsJsonAsync("/api/customer/me/profile", body, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var errors = await ReadErrorsAsync(response);
        errors.Should().ContainKey("dateOfBirth");
    }

    // ─── Valid update → 200 OK (regression: valid updates still work) ─────────

    [Fact]
    public async Task PATCH_MeProfile_ValidFields_Returns200()
    {
        var email = $"val-ok-{Guid.NewGuid():N}@example.com";
        var (client, _) = await CreateCustomerAsync(email);

        var body = new
        {
            firstName = "Updated",
            lastName = "Name",
            email,
            gender = 2,  // Female
            dateOfBirth = "1990-05-12"
        };
        var response = await client.PatchAsJsonAsync("/api/customer/me/profile", body, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── Profile unchanged after validation failure ───────────────────────────

    [Fact]
    public async Task PATCH_MeProfile_AfterValidationFailure_ProfileRemainsUnchanged()
    {
        var email = $"val-unchanged-{Guid.NewGuid():N}@example.com";
        var (client, customerId) = await CreateCustomerAsync(email);

        // Attempt invalid update (empty name)
        var invalidBody = new
        {
            firstName = "",
            lastName = "Tester",
            email,
            gender = 1
        };
        var invalidResponse = await client.PatchAsJsonAsync("/api/customer/me/profile", invalidBody, JsonOpts);
        invalidResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Verify original profile is unchanged
        var profileResponse = await client.GetAsync("/api/customer/me");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = await JsonDocument.ParseAsync(await profileResponse.Content.ReadAsStreamAsync());
        var firstName = doc.RootElement.GetProperty("firstName").GetString();
        firstName.Should().Be("Validation", because: "invalid update must not change saved state");
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private static async Task<Dictionary<string, string[]>> ReadErrorsAsync(HttpResponseMessage response)
    {
        try
        {
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                var result = new Dictionary<string, string[]>();
                foreach (var prop in errors.EnumerateObject())
                {
                    result[prop.Name] = prop.Value.EnumerateArray()
                        .Select(v => v.GetString() ?? string.Empty)
                        .ToArray();
                }
                return result;
            }
        }
        catch { /* swallow */ }

        return new Dictionary<string, string[]>();
    }
}
