using FluentAssertions;

namespace CookieCommerce.Web.Tests;

public sealed class InitialTestDiscoveryTests
{
    [Fact]
    public void TestProject_IsDiscoverable()
    {
        true.Should().BeTrue();
    }
}
