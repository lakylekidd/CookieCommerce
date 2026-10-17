using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace CookieCommerce.Web.Tests.Services;

public class CustomerAddressServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task GetAddressesAsync_CallsCustomerDomainAddressesEndpoint_GivenSignedInCustomer()
    {
        HttpRequestMessage? captured = null;
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[]
            {
                new
                {
                    id = Guid.NewGuid(),
                    index = 1,
                    icon = "home",
                    label = "Home",
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
                }
            }, options: JsonOptions)
        }, request => captured = request);

        var addresses = await service.GetAddressesAsync();

        captured.Should().NotBeNull();
        captured!.RequestUri!.PathAndQuery.Should().Be("/api/customer/me/addresses");
        captured.Headers.GetValues("X-Customer-Id").Should().ContainSingle();
        addresses.Should().ContainSingle();
        addresses.Single().Label.Should().Be("Home");
    }

    [Fact]
    public async Task GetAddressesAsync_ReturnsEmptyWithoutFakeFallback_GivenCustomerDomainFails()
    {
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var addresses = await service.GetAddressesAsync();

        addresses.Should().BeEmpty("CustomerDomain failures must not fall back to fake/static address data");
    }

    [Fact]
    public async Task GetPrimaryAddressAsync_CallsCustomerDomainDefaultEndpoint_GivenSignedInCustomer()
    {
        HttpRequestMessage? captured = null;
        var id = Guid.NewGuid();
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                id,
                index = 1,
                icon = "home",
                label = "Home",
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
            }, options: JsonOptions)
        }, request => captured = request);

        var address = await service.GetPrimaryAddressAsync();

        captured.Should().NotBeNull();
        captured!.RequestUri!.PathAndQuery.Should().Be("/api/customer/me/addresses/default");
        address.Should().NotBeNull();
        address!.Id.Should().Be(id);
        address.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task GetPrimaryAddressAsync_ReturnsNullWithoutFakeFallback_GivenNoDefaultAddress()
    {
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.NoContent));

        var address = await service.GetPrimaryAddressAsync();

        address.Should().BeNull("no-default responses must render empty state instead of fake primary address data");
    }

    private static CustomerDomainAccountService BuildService(
        HttpResponseMessage response,
        Action<HttpRequestMessage>? capture = null)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capture?.Invoke(request);
                return Task.FromResult(response);
            });

        var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://customer-domain-api/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient("customer-domain-api")).Returns(client);

        var userId = Guid.NewGuid();
        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        authService.Setup(x => x.CurrentUser).Returns(new UserDto
        {
            Id = userId,
            Email = "jamie@example.com",
            FirstName = "Jamie",
            LastName = "Baker"
        });



        return new CustomerDomainAccountService(
            factory.Object,
            authService.Object,
            null!,
            NullLogger<CustomerDomainAccountService>.Instance);
    }
}
