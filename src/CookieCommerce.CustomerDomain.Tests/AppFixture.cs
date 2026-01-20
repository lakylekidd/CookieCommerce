
using Alba;
using Marten;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace CookieCommerce.CustomerDomain.Tests;

public class AppFixture : IAsyncLifetime
{
    private string SchemaName { get; } = "sch" + Guid.NewGuid().ToString().Replace("-", string.Empty);
    public IAlbaHost Host { get; private set; }

    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        // This is bootstrapping the actual application using
        // its implied Program.Main() set up
        Host = await AlbaHost.For<Program>(b =>
        {
            b.ConfigureServices((context, services) =>
            {
                // Important! You can make your test harness work a little faster (important on its own)
                // and probably be more reliable by overriding your Marten configuration to run all
                // async daemons in "Solo" mode so they spin up faster and there's no issues from
                // PostgreSQL having trouble with advisory locks when projections are rapidly started and stopped

                // This was added in V8.8
                services.MartenDaemonModeIsSolo();
            });
        });
    }
}
