using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Logs outbound HTTP dependency outcomes without recording request URLs, bodies, headers, or response bodies.
/// </summary>
public sealed class OperationalHttpLoggingHandler(ILogger<OperationalHttpLoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var dependency = request.RequestUri?.Host ?? "unknown";
        var method = request.Method.Method;

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogDebug(
                    "Outbound HTTP dependency request completed [dependency={Dependency}, method={Method}, statusCode={StatusCode}]",
                    dependency, method, (int)response.StatusCode);
            }
            else
            {
                logger.LogWarning(
                    "Outbound HTTP dependency returned a non-success status [dependency={Dependency}, method={Method}, statusCode={StatusCode}]",
                    dependency, method, (int)response.StatusCode);
            }

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Outbound HTTP dependency request failed [dependency={Dependency}, method={Method}, failureType={FailureType}]",
                dependency, method, exception.GetType().Name);
            throw;
        }
    }
}
