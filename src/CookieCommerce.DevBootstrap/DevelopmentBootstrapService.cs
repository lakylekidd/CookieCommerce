using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.DevBootstrap;

public sealed class DevelopmentBootstrapService(
    IServiceScopeFactory scopeFactory,
    ILogger<DevelopmentBootstrapService> logger,
    IHostEnvironment? hostEnvironment = null,
    IConfiguration? configuration = null) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (hostEnvironment?.IsDevelopment() != true || string.Equals(configuration?["DevelopmentBootstrap:Enabled"], "false", StringComparison.OrdinalIgnoreCase))
            return;
        logger.LogInformation("Development bootstrap starting");

        using var scope = scopeFactory.CreateScope();
        var identityBootstrapAdapter = scope.ServiceProvider.GetService<IDevelopmentIdentityBootstrapAdapter>();
        if (identityBootstrapAdapter is not null)
        {
            foreach (var customer in DevelopmentBootstrapData.Customers)
            {
                await EnsureUserWithRetryAsync(
                    identityBootstrapAdapter,
                    customer.Email,
                    customer.FirstName,
                    customer.LastName,
                    customer.Password,
                    customer.Role,
                    stoppingToken);
            }
        }

        var productBootstrapAdapter = scope.ServiceProvider.GetService<IDevelopmentProductBootstrapAdapter>();
        if (productBootstrapAdapter is not null && (hostEnvironment?.IsDevelopment() ?? false))
        {
            var staticWebRootPath = ResolveStaticWebRootPath();
            ValidateProductMediaDefaults(staticWebRootPath);

            foreach (var product in DevelopmentBootstrapData.Products)
            {
                await productBootstrapAdapter.EnsureProductAsync(product, staticWebRootPath, stoppingToken);
            }
        }

        logger.LogInformation("Development bootstrap complete");
    }

    public static string ResolveStaticWebRootMediaPath(string staticWebRootPath, string sourcePath)
    {
        var relativePath = sourcePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(staticWebRootPath, relativePath));
    }

    private string ResolveStaticWebRootPath()
    {
        var configuredPath = configuration?["DevelopmentBootstrap:StaticWebRootPath"];
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return Path.GetFullPath(configuredPath);

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "CookieCommerce.Web", "wwwroot");
            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Development bootstrap could not resolve the Web static asset root. Configure DevelopmentBootstrap:StaticWebRootPath to the CookieCommerce.Web/wwwroot directory.");
    }

    private static void ValidateProductMediaDefaults(string staticWebRootPath)
    {
        foreach (var product in DevelopmentBootstrapData.Products)
        {
            if (product.Variants.Count == 0)
            {
                throw new InvalidOperationException($"Development bootstrap product '{product.ProductReference}' has no variants configured.");
            }

            if (product.MainImage is not null)
            {
                ValidateMediaExists(staticWebRootPath, product.ProductReference, null, product.MainImage);
            }

            foreach (var variant in product.Variants)
            {
                foreach (var photo in variant.Photos)
                {
                    ValidateMediaExists(staticWebRootPath, product.ProductReference, variant.Name, photo);
                }
            }
        }
    }

    private static void ValidateMediaExists(
        string staticWebRootPath,
        string productReference,
        string? variantName,
        DevelopmentBootstrapMediaAsset mediaAsset)
    {
        var path = ResolveStaticWebRootMediaPath(staticWebRootPath, mediaAsset.SourcePath);
        if (File.Exists(path))
            return;

        var variantContext = string.IsNullOrWhiteSpace(variantName) ? "product main image" : $"variant '{variantName}'";
        throw new FileNotFoundException(
            $"Development bootstrap missing media for product '{productReference}', {variantContext}: '{mediaAsset.SourcePath}'. Resolved path: '{path}'.",
            path);
    }

    private async Task EnsureUserWithRetryAsync(
        IDevelopmentIdentityBootstrapAdapter identityBootstrapAdapter,
        string email,
        string firstName,
        string lastName,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 15; attempt++)
        {
            try
            {
                await identityBootstrapAdapter.EnsureUserAsync(email, firstName, lastName, password, role, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 15)
            {
                logger.LogWarning("Development bootstrap could not ensure a seed identity [attempt={Attempt}, failureType={FailureType}]; retrying", attempt, ex.GetType().Name);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        await identityBootstrapAdapter.EnsureUserAsync(email, firstName, lastName, password, role, cancellationToken);
    }
}

public interface IDevelopmentIdentityBootstrapAdapter
{
    Task EnsureUserAsync(
        string email,
        string firstName,
        string lastName,
        string password,
        string role,
        CancellationToken cancellationToken);
}

public interface IDevelopmentProductBootstrapAdapter
{
    Task EnsureProductAsync(
        DevelopmentBootstrapProduct productDefault,
        string staticWebRootPath,
        CancellationToken cancellationToken);
}
