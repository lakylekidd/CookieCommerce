using CookieCommerce.Web;
using CookieCommerce.Web.Components.Brandbook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace CookieCommerce.Web.Tests.Components.Brandbook;

public class BrandbookTests
{
    [Theory]
    [InlineData("Development", true, true)]
    [InlineData("Development", false, false)]
    [InlineData("Production", true, false)]
    public void Availability_requires_development_and_explicit_opt_in(string environment, bool enabled, bool expected)
    {
        var host = Mock.Of<IHostEnvironment>(value => value.EnvironmentName == environment);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Brandbook:Enabled"] = enabled.ToString()
        }).Build();

        Assert.Equal(expected, BrandbookAvailability.IsEnabled(host, configuration));
    }

    [Fact]
    public void Catalog_finds_matching_elements_within_selected_domain()
    {
        var results = BrandbookCatalog.Filter("Backoffice", "table").ToList();

        Assert.Single(results);
        Assert.Equal("backoffice-table", results[0].Id);
        Assert.Equal("Backoffice", results[0].Domain);
    }

    [Fact]
    public void Catalog_entries_have_unique_ids_and_all_ui_domains()
    {
        Assert.Equal(BrandbookCatalog.Entries.Count, BrandbookCatalog.Entries.Select(entry => entry.Id).Distinct().Count());
        Assert.Equal(
            ["Shared", "Web Shop", "Backoffice", "Customer Dashboard"],
            BrandbookCatalog.Entries.Select(entry => entry.Domain).Distinct());
    }

}
