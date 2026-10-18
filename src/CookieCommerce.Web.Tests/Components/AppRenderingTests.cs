using FluentAssertions;

namespace CookieCommerce.Web.Tests.Components;

public sealed class AppRenderingTests
{
    [Fact]
    public void Does_not_prerender_authenticated_routes_before_browser_storage_is_available()
    {
        var repositoryRoot = FindRepositoryRoot();
        var appSource = File.ReadAllText(Path.Combine(
            repositoryRoot.FullName,
            "src",
            "CookieCommerce.Web",
            "Components",
            "App.razor"));

        appSource.Should().Contain("<Routes @rendermode=\"new InteractiveServerRenderMode(prerender: false)\" />");
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "src", "CookieCommerce.slnx")))
                return current;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate CookieCommerce repository root from test output directory.");
    }
}
