using System.Net;
using System.Text.Json;
using CookieCommerce.AuthenticationManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CookieCommerce.AuthenticationManagement.Api.Middleware;

/// <summary>
/// Middleware that catches domain exceptions and maps them to appropriate HTTP status codes
/// </summary>
public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            AuthUserNotFoundException notFound => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Auth User Not Found",
                    Detail = notFound.Message,
                    Instance = context.Request.Path
                }
            ),
            InvalidCredentialsException invalidCredentials => (
                HttpStatusCode.Unauthorized,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Title = "Invalid Credentials",
                    Detail = invalidCredentials.Message,
                    Instance = context.Request.Path
                }
            ),
            EmailAlreadyTakenException emailTaken => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Email Already Taken",
                    Detail = emailTaken.Message,
                    Instance = context.Request.Path
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "An error occurred while processing your request",
                    Detail = exception.Message,
                    Instance = context.Request.Path
                }
            )
        };

        LogFailure(context, exception, statusCode);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private void LogFailure(HttpContext context, Exception exception, HttpStatusCode statusCode)
    {
        if ((int)statusCode >= 500)
        {
            _logger.LogError(
                "Authentication Management API request failed [traceId={TraceId}, method={Method}, statusCode={StatusCode}, exceptionType={ExceptionType}]",
                context.TraceIdentifier, context.Request.Method, (int)statusCode, exception.GetType().Name);
            return;
        }

        _logger.LogWarning(
            "Authentication Management API request was rejected [traceId={TraceId}, method={Method}, statusCode={StatusCode}, exceptionType={ExceptionType}]",
            context.TraceIdentifier, context.Request.Method, (int)statusCode, exception.GetType().Name);
    }
}

/// <summary>
/// Extension method to register the domain exception middleware
/// </summary>
public static class DomainExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseDomainExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DomainExceptionMiddleware>();
    }
}
