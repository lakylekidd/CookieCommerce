using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace CookieCommerce.Web.Tests.Services;

public class CustomerAddressAddServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Fact]
    public async Task AddAddressAsync_PostsCurrentCustomerAddress_GivenValidAddress()
    {
        HttpRequestMessage? captured = null;
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.Created), request => captured = request);

        var result = await service.AddAddressAsync(BuildAddress());

        result.Success.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.RequestUri!.PathAndQuery.Should().Be("/api/customer/me/addresses");
        captured.Headers.GetValues("X-Customer-Id").Should().ContainSingle();
    }

    [Fact]
    public async Task AddAddressAsync_ReturnsFieldErrors_GivenValidationProblem()
    {
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = JsonContent.Create(new { errors = new Dictionary<string, string[]> { ["label"] = ["Label is required."] } }, options: JsonOptions)
        });

        var result = await service.AddAddressAsync(BuildAddress());

        result.Success.Should().BeFalse();
        result.FieldErrors.Should().ContainKey("label");
    }

    [Fact]
    public async Task AddAddressAsync_ReturnsConflictMessage_GivenDuplicateLabel()
    {
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.Conflict));

        var result = await service.AddAddressAsync(BuildAddress());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("label");
    }

    private static AddressBookItemDto BuildAddress() => new()
    {
        Icon = "home",
        Label = "Home",
        FirstName = "Jamie",
        LastName = "Baker",
        Street = "Cookie Lane",
        HouseNumber = "12",
        City = "Baker City",
        PostalCode = "1234 AB",
        Province = "North Brabant",
        Country = "Netherlands",
        Phone = "+31612345678",
        IsDefault = true
    };

    private static CustomerDomainAccountService BuildService(HttpResponseMessage response, Action<HttpRequestMessage>? capture = null)
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

        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        authService.Setup(x => x.CurrentUser).Returns(new UserDto { Id = Guid.NewGuid(), Email = "jamie@example.com" });

        return new CustomerDomainAccountService(factory.Object, authService.Object, null!, NullLogger<CustomerDomainAccountService>.Instance);
    }
}
