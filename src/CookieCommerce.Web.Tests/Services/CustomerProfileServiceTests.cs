using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace CookieCommerce.Web.Tests.Services;

public class CustomerProfileServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static (CustomerProfileService Service, CustomerProfileState State) Build(
        HttpResponseMessage getResponse,
        HttpResponseMessage? patchResponse = null)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(getResponse);

        if (patchResponse is not null)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Patch),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(patchResponse);
        }

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
        var service = new CustomerProfileService(
            mockFactory.Object,
            mockAuth.Object,
            state,
            NullLogger<CustomerProfileService>.Instance);

        return (service, state);
    }

    private static StringContent ProfileJson(object profile) =>
        new(JsonSerializer.Serialize(profile, JsonOptions), System.Text.Encoding.UTF8, "application/json");

    [Fact]
    public async Task GetProfileAsync_ReturnsProfile_WhenApiSucceeds()
    {
        var apiResponse = new
        {
            id = Guid.NewGuid(),
            firstName = "Jamie",
            lastName = "Baker",
            email = "jamie@example.com",
            pendingEmail = (string?)null,
            emailVerificationStatus = "Verified",
            gender = (int?)null,
            dateOfBirth = (string?)null
        };

        var (service, _) = Build(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = ProfileJson(apiResponse)
        });

        var result = await service.GetProfileAsync();

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jamie");
        result.LastName.Should().Be("Baker");
        result.Email.Should().Be("jamie@example.com");
        result.PendingEmail.Should().BeNull();
        result.EmailVerificationStatus.Should().Be("Verified");
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsNull_WhenApiReturns401()
    {
        var (service, _) = Build(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var result = await service.GetProfileAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsNull_WhenApiReturns404()
    {
        var (service, _) = Build(new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await service.GetProfileAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_DoesNotUseMockFallback()
    {
        // When API returns 404, result must be null — not a static/mock profile
        var (service, _) = Build(new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await service.GetProfileAsync();
        result.Should().BeNull("mock fallback is not allowed for customer-facing profile load");
    }

    [Fact]
    public async Task GetProfileAsync_CachesResultInProfileState()
    {
        var profileId = Guid.NewGuid();
        var apiResponse = new
        {
            id = profileId,
            firstName = "Jamie",
            lastName = "Baker",
            email = "jamie@example.com",
            pendingEmail = (string?)null,
            emailVerificationStatus = "Verified",
            gender = (int?)null,
            dateOfBirth = (string?)null
        };

        var (service, state) = Build(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = ProfileJson(apiResponse)
        });

        await service.GetProfileAsync();

        state.Profile.Should().NotBeNull();
        state.Profile!.Id.Should().Be(profileId);
    }

    [Fact]
    public async Task UpdateProfileAsync_ReturnsSuccess_WhenApiSucceeds()
    {
        var profileId = Guid.NewGuid();
        var updatedApi = new
        {
            id = profileId,
            firstName = "NewFirst",
            lastName = "NewLast",
            email = "jamie@example.com",
            pendingEmail = (string?)null,
            emailVerificationStatus = "Verified",
            gender = (int?)null,
            dateOfBirth = (string?)null
        };

        var (service, state) = Build(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = ProfileJson(updatedApi) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = ProfileJson(updatedApi) });

        var profile = new UserProfileDto
        {
            Id = profileId,
            FirstName = "NewFirst",
            LastName = "NewLast",
            Email = "jamie@example.com"
        };

        var result = await service.UpdateProfileAsync(profile);

        result.Success.Should().BeTrue();
        state.Profile.Should().NotBeNull();
        state.Profile!.FirstName.Should().Be("NewFirst");
    }

    [Fact]
    public async Task UpdateProfileAsync_ReturnsFail_WhenApiReturns409()
    {
        var (service, _) = Build(
            new HttpResponseMessage(HttpStatusCode.OK),
            new HttpResponseMessage(HttpStatusCode.Conflict));

        var result = await service.UpdateProfileAsync(new UserProfileDto { Email = "dup@example.com" });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already in use");
    }

    [Fact]
    public async Task UpdateProfileAsync_ReturnsFail_WhenApiFailsWithServerError()
    {
        var (service, _) = Build(
            new HttpResponseMessage(HttpStatusCode.OK),
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await service.UpdateProfileAsync(new UserProfileDto { Email = "test@example.com" });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task HasBirthdateAsync_ReturnsTrue_WhenProfileHasDateOfBirth()
    {
        var apiResponse = new
        {
            id = Guid.NewGuid(),
            firstName = "Jamie",
            lastName = "Baker",
            email = "jamie@example.com",
            pendingEmail = (string?)null,
            emailVerificationStatus = "Verified",
            gender = (int?)null,
            dateOfBirth = "1990-05-12"
        };

        var (service, _) = Build(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = ProfileJson(apiResponse)
        });

        var hasBirthdate = await service.HasBirthdateAsync();

        hasBirthdate.Should().BeTrue();
    }

    [Fact]
    public async Task HasBirthdateAsync_ReturnsFalse_WhenProfileHasNullDateOfBirth()
    {
        var apiResponse = new
        {
            id = Guid.NewGuid(),
            firstName = "Jamie",
            lastName = "Baker",
            email = "jamie@example.com",
            pendingEmail = (string?)null,
            emailVerificationStatus = "Verified",
            gender = (int?)null,
            dateOfBirth = (string?)null
        };

        var (service, _) = Build(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = ProfileJson(apiResponse)
        });

        var hasBirthdate = await service.HasBirthdateAsync();

        hasBirthdate.Should().BeFalse();
    }
}
