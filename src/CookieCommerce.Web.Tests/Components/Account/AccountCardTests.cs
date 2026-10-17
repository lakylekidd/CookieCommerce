using CookieCommerce.Web.Components.UI.CustomerDashboard;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CookieCommerce.Web.Tests.Components.Account;

public class AccountCardTests
{
    [Fact]
    public async Task AccountCard_RendersProvidedContentInsideDashboardCard()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(AccountCard.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Profile content"))
        });

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<AccountCard>(parameters)).ToHtmlString());

        html.Should().Contain("class=\"dashboard-card\"");
        html.Should().Contain("Profile content");
    }
}
