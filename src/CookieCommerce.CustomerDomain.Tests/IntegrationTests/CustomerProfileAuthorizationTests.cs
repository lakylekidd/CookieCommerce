using System.Net;
using System.Net.Http.Json;
using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

[Collection("Integration")]
public class CustomerProfileAuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CustomerProfileAuthorizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Me_ProfileEndpoints_ReturnUnauthorized_GivenUnauthenticatedRequest()
    {
        var client = _factory.CreateClient();

        var getResponse = await client.GetAsync("/api/customer/me");
        var patchResponse = await client.PatchAsJsonAsync("/api/customer/me/profile", new
        {
            firstName = "Jamie",
            lastName = "Baker",
            email = "jamie@example.com"
        });
        var resendResponse = await client.PostAsJsonAsync("/api/customer/me/email-verification", new { });
        var confirmResponse = await client.PostAsJsonAsync("/api/customer/me/email-verification/confirm", new { token = "token" });

        getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        patchResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        resendResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void ResolveOwner_ThrowsUnauthorizedAccess_GivenCrossCustomerProfileAccess()
    {
        var resolver = new CurrentCustomerResolver();
        var authenticatedCustomerId = Guid.NewGuid();
        var requestedCustomerId = Guid.NewGuid();

        Action act = () => resolver.ResolveOwner(authenticatedCustomerId, requestedCustomerId);

        act.Should().Throw<UnauthorizedCustomerProfileAccessException>();
    }
}
