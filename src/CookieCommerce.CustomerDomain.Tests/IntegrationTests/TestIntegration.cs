namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class TestIntegration : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TestIntegration(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCustomers_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/customer");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
