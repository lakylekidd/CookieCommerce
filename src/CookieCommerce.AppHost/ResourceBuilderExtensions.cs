using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace CookieCommerce.AppHost;

internal static class ResourceBuilderExtensions
{
    internal static IResourceBuilder<T> WithBrandbookUi<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        return builder.WithCommand(
            name: "brandbook-ui",
            displayName: "Open UI Brandbook",
            executeCommand: _ =>
            {
                try
                {
                    var url = $"{builder.GetEndpoint("http").Url.TrimEnd('/')}/design-system";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    return Task.FromResult(new ExecuteCommandResult { Success = true });
                }
                catch (Exception e)
                {
                    return Task.FromResult(new ExecuteCommandResult { Success = false, ErrorMessage = e.Message });
                }
            },
            commandOptions: new CommandOptions
            {
                Description = "Open the development UI reference",
                IconName = "Document",
                IconVariant = IconVariant.Filled,
                UpdateState = context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy
                    ? ResourceCommandState.Enabled
                    : ResourceCommandState.Disabled
            });
    }

    internal static IResourceBuilder<T> WithScalarUi<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
    {
        return builder.WithOpenApiDocs(
            name: "scalar-ui-docs",
            displayName: "Scalar UI Docs",
            openApiUiPath: "scalar/v1");
    }

    private static IResourceBuilder<T> WithOpenApiDocs<T>(this IResourceBuilder<T> builder,
        string name,
        string displayName,
        string openApiUiPath)
        where T : IResourceWithEndpoints
    {
        return builder.WithCommand(
                name: name,
                displayName: displayName,
                executeCommand: async _ =>
                {
                    try
                    {
                        var endpoint = builder.GetEndpoint("https");
                        var url = $"{endpoint.Url}/{openApiUiPath}";
                        Process.Start(new ProcessStartInfo(url)
                        {
                            UseShellExecute = true
                        });
                        return new ExecuteCommandResult { Success = true };
                    }
                    catch (Exception e)
                    {
                        return new ExecuteCommandResult { Success = false, ErrorMessage = e.Message };
                    }
                },
                commandOptions: new CommandOptions
                {
                    Description = "Open the API UI",
                    IconName = "Document",
                    IconVariant = IconVariant.Filled,
                    UpdateState = context =>
                    {
                        return context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy
                        ? ResourceCommandState.Enabled
                        : ResourceCommandState.Disabled;
                    }
                }
            );
    }
}
