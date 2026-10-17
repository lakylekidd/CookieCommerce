using System.Net;
using CookieCommerce.Web.Models.Auth;
using CookieCommerce.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;

namespace CookieCommerce.Web.Tests.Services;

public class CustomerAddressDeleteServiceTests
{
    [Fact]
    public async Task DeleteAddressAsync_DeletesByStableAddressId_GivenSignedInCustomer()
    {
        HttpRequestMessage? captured = null;
        var addressId = Guid.CreateVersion7();
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.NoContent), request => captured = request);

        var result = await service.DeleteAddressAsync(addressId);

        result.Success.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Method.Should().Be(HttpMethod.Delete);
        captured.RequestUri!.PathAndQuery.Should().Be($"/api/customer/me/addresses/{addressId}");
    }

    [Fact]
    public async Task DeleteAddressAsync_ReturnsNotFound_GivenMissingAddress()
    {
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await service.DeleteAddressAsync(Guid.CreateVersion7());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task SetAsPrimaryAsync_UpdatesDefaultAddress_GivenSignedInCustomer()
    {
        var service = BuildService(new HttpResponseMessage(HttpStatusCode.NoContent));

        var result = await service.SetAsPrimaryAsync(Guid.CreateVersion7());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("updated");
    }

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
