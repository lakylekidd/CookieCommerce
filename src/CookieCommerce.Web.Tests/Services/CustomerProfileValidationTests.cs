using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace CookieCommerce.Web.Tests.Services;

/// <summary>
/// US2 – Web service validation mapping tests:
/// field-level messages surfaced from 422 responses, unchanged form state verified
/// via ResultDto.FieldErrors, and 409 conflict mapped to email field error.
/// </summary>
public class CustomerProfileValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static CustomerProfileService Build(HttpResponseMessage patchResponse)
    {
        var getResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                id = Guid.NewGuid(),
                firstName = "Jamie",
                lastName = "Baker",
                email = "jamie@example.com",
                pendingEmail = (string?)null,
                emailVerificationStatus = "Verified",
                gender = (string?)null,
                dateOfBirth = (string?)null
            }, JsonOptions), Encoding.UTF8, "application/json")
        };

        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(getResponse);

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(patchResponse);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://customer-domain-api/")
        };

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient("customer-domain-api")).Returns(httpClient);

        var mockAuth = new Mock<IAuthService>();
        mockAuth.Setup(a => a.InitializeAsync()).Returns(Task.CompletedTask);
        mockAuth.Setup(a => a.CurrentUser).Returns(new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "jamie@example.com",
            FirstName = "Jamie",
            LastName = "Baker"
        });

        var state = new CustomerProfileState();
        return new CustomerProfileService(
            mockFactory.Object,
            mockAuth.Object,
            state,
            NullLogger<CustomerProfileService>.Instance);
    }

    private static StringContent ValidationProblemJson(Dictionary<string, string[]> fieldErrors)
    {
        var body = new
        {
            status = 422,
            title = "Profile Validation Failed",
            errors = fieldErrors
        };
        return new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/problem+json");
    }

    // ─── 422 with field errors → ResultDto.FieldErrors populated ─────────────

    [Fact]
    public async Task UpdateProfileAsync_422WithFirstNameError_ReturnsFieldLevelError()
    {
        var patchResponse = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = ValidationProblemJson(new Dictionary<string, string[]>
            {
                ["firstName"] = ["First name is required."]
            })
        };

        var service = Build(patchResponse);
        var result = await service.UpdateProfileAsync(new UserProfileDto
        {
            FirstName = "",
            LastName = "Baker",
            Email = "jamie@example.com"
        });

        result.Success.Should().BeFalse();
        result.FieldErrors.Should().ContainKey("firstName");
        result.FieldErrors["firstName"].Should().Contain("First name is required.");
    }

    [Fact]
    public async Task UpdateProfileAsync_422WithEmailError_ReturnsEmailFieldError()
    {
        var patchResponse = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = ValidationProblemJson(new Dictionary<string, string[]>
            {
                ["email"] = ["Email address is not valid."]
            })
        };

        var service = Build(patchResponse);
        var result = await service.UpdateProfileAsync(new UserProfileDto
        {
            FirstName = "Jamie",
            LastName = "Baker",
            Email = "bad-email"
        });

        result.Success.Should().BeFalse();
        result.FieldErrors.Should().ContainKey("email");
    }

    [Fact]
    public async Task UpdateProfileAsync_422WithMultipleErrors_ReturnsAllFieldErrors()
    {
        var patchResponse = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = ValidationProblemJson(new Dictionary<string, string[]>
            {
                ["firstName"] = ["First name is required."],
                ["lastName"] = ["Last name is required."],
                ["email"] = ["Email address is not valid."],
                ["dateOfBirth"] = ["Date of birth must be in the past."]
            })
        };

        var service = Build(patchResponse);
        var result = await service.UpdateProfileAsync(new UserProfileDto
        {
            FirstName = "",
            LastName = "",
            Email = "bad"
        });

        result.Success.Should().BeFalse();
        result.FieldErrors.Should().ContainKey("firstName");
        result.FieldErrors.Should().ContainKey("lastName");
        result.FieldErrors.Should().ContainKey("email");
        result.FieldErrors.Should().ContainKey("dateOfBirth");
    }

    // ─── 409 Conflict → email field error ────────────────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_409Conflict_ReturnsEmailFieldError()
    {
        var patchResponse = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { status = 409, title = "Email Address Conflict" }),
                Encoding.UTF8,
                "application/problem+json")
        };

        var service = Build(patchResponse);
        var result = await service.UpdateProfileAsync(new UserProfileDto
        {
            FirstName = "Jamie",
            LastName = "Baker",
            Email = "taken@example.com"
        });

        result.Success.Should().BeFalse();
        result.FieldErrors.Should().ContainKey("email",
            because: "duplicate email conflict must surface as an email field error for the UI");
        result.FieldErrors["email"].Should().NotBeEmpty();
    }

    // ─── Unchanged form state after failure ───────────────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_ValidationFailure_ProfileStateNotUpdated()
    {
        var patchResponse = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = ValidationProblemJson(new Dictionary<string, string[]>
            {
                ["firstName"] = ["First name is required."]
            })
        };

        var service = Build(patchResponse);

        var profile = new UserProfileDto
        {
            FirstName = "",
            LastName = "Baker",
            Email = "jamie@example.com"
        };

        var result = await service.UpdateProfileAsync(profile);

        result.Success.Should().BeFalse();
        // Profile object passed in should be unchanged (service does not mutate it)
        profile.FirstName.Should().Be("", because: "service must not alter the form model on failure");
    }

    // ─── 200 OK still works after validation changes ─────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_200Ok_ReturnsSuccess()
    {
        var updatedProfile = new
        {
            id = Guid.NewGuid(),
            firstName = "Jamie",
            lastName = "Baker",
            email = "jamie@example.com",
            pendingEmail = (string?)null,
            emailVerificationStatus = "Verified",
            gender = (string?)null,
            dateOfBirth = (string?)null
        };
        var patchResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(updatedProfile, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        var service = Build(patchResponse);
        var result = await service.UpdateProfileAsync(new UserProfileDto
        {
            FirstName = "Jamie",
            LastName = "Baker",
            Email = "jamie@example.com"
        });

        result.Success.Should().BeTrue();
        result.FieldErrors.Should().BeEmpty();
    }
}
