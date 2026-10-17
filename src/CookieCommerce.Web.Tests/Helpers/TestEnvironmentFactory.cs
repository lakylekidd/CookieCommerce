using Microsoft.AspNetCore.Hosting;
using Moq;

namespace CookieCommerce.Web.Tests.Helpers;

internal static class TestEnvironmentFactory
{
    public static IWebHostEnvironment CreateTempWebHostEnvironment()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "cookie-commerce-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempPath);

        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(e => e.WebRootPath).Returns(tempPath);
        return mock.Object;
    }
}
