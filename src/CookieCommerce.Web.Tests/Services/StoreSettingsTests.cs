using CookieCommerce.Web.Services;
using CookieCommerce.Web.Services.Commerce;
using CookieCommerce.Web.Tests.Fixtures;
using FluentAssertions;
using Moq;

namespace CookieCommerce.Web.Tests.Services;

public sealed class StoreSettingsTests
{
    [Fact]
    public async Task Coming_soon_mode_is_enabled_for_a_new_or_legacy_store()
    {
        var store = new MemoryCommerceStore();
        await store.SaveHistoricalJsonAsync<CommerceSettings>("store", "{\"OrderReferencePrefix\":\"TGR\"}");

        var service = CreateService(store);

        (await service.IsComingSoonEnabledAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task Administrator_can_make_the_store_public()
    {
        var store = new MemoryCommerceStore();
        var service = CreateService(store);

        await service.SaveAsync(new CommerceSettings { ComingSoonEnabled = false });

        (await service.IsComingSoonEnabledAsync()).Should().BeFalse();
    }

    private static StoreSettingsService CreateService(ICommerceStore store)
    {
        var auth = new Mock<IAuthService>();
        auth.Setup(service => service.InitializeAsync()).Returns(Task.CompletedTask);
        auth.SetupGet(service => service.IsAdmin).Returns(true);
        return new StoreSettingsService(store, new CommerceAccess(auth.Object));
    }
}
