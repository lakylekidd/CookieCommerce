using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CookieCommerce.Web;

public static class BrandbookAvailability
{
    public static bool IsEnabled(IHostEnvironment environment, IConfiguration configuration) =>
        environment.IsDevelopment() && configuration.GetValue<bool>("Brandbook:Enabled");
}
